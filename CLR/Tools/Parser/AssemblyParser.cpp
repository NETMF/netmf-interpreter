////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"


////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::SetReference( MetaData::mdTokenSet& m, mdToken d )
{
    if(IsNilToken(d) == false)
    {
        m.insert( d );
    }
}

template <class T> void RemoveUnusedItems( std::map<mdToken,T>& d, MetaData::mdTokenSet& s )
{
    MetaData::mdTokenSet     setAll;
    MetaData::mdTokenSetIter setIt;
    std::map<mdToken,T>::iterator  it;


    //
    // First build a set of the token in the destination.
    //
    for(it = d.begin(); it != d.end(); it++)
    {
        setAll.insert( it->first );
    }

    //
    // Then remove the one that are used.
    //
    for(setIt = s.begin(); setIt != s.end(); setIt++)
    {
        setAll.erase( *setIt );
    }

    //
    // What's left are the items that are not used, remove them.
    //
    for(setIt = setAll.begin(); setIt != setAll.end(); setIt++)
    {
        d.erase( *setIt );
    }
}

void RemoveUnusedItems( MetaData::TypeDefMap& d, MetaData::mdTokenSet& s )
{
    MetaData::mdTokenSet     setAll;
    MetaData::mdTokenSetIter setIt;
    MetaData::TypeDefMap::iterator  it;


    //
    // First build a set of the token in the destination.
    //
    for(it = d.begin(); it != d.end(); it++)
    {
        setAll.insert( it->second.m_td );
    }

    //
    // Then remove the one that are used.
    //
    for(setIt = s.begin(); setIt != s.end(); setIt++)
    {
        setAll.erase( *setIt );
    }

    //
    // What's left are the items that are not used, remove them.
    //
    for(setIt = setAll.begin(); setIt != setAll.end(); setIt++)
    {
        d.RemoveByToken( *setIt );
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::TypeSignature::Init()
{
                                            // Parser*        m_holder;
                                            //
    m_opt             = ELEMENT_TYPE_END;   // CorElementType m_opt;
    m_optTypeModifier = ELEMENT_TYPE_END;   // CorElementType type modifier. 
    m_token           = mdTokenNil      ;   // mdToken        m_token;
    m_sub             = NULL            ;   // TypeSignature* m_sub;
    m_rank            = 0               ;   // int            m_rank;
                                            // LimitList      m_sizes;
                                            // LimitList      m_lowBounds;
}

void MetaData::TypeSignature::Clean()
{
    if(m_sub)
    {
        delete m_sub; m_sub = NULL;
    }

    m_sizes    .clear();
    m_lowBounds.clear();
}

MetaData::TypeSignature::TypeSignature( Parser* holder )
{
    m_holder = holder;

    Init();
}

MetaData::TypeSignature::TypeSignature( const MetaData::TypeSignature& ts )
{
    Init();

    *this = ts;
}

MetaData::TypeSignature& MetaData::TypeSignature::operator= ( const MetaData::TypeSignature& ts )
{
    Clean();

    m_holder            = ts.m_holder   ; // Parser*        m_holder;
                                  //
    m_opt               = ts.m_opt      ; // CorElementType m_opt;
    m_optTypeModifier   = ts.m_optTypeModifier;
    m_token             = ts.m_token    ; // mdToken        m_token;
    m_sub               = NULL          ; // TypeSignature* m_sub;
    m_rank              = ts.m_rank     ; // int            m_rank;
    m_sizes             = ts.m_sizes    ; // LimitList      m_sizes;
    m_lowBounds         = ts.m_lowBounds; // LimitList      m_lowBounds;

    if(ts.m_sub)
    {
        m_sub = new TypeSignature( *ts.m_sub );
    }

    return *this;
}

MetaData::TypeSignature::~TypeSignature()
{
    Clean();
}

void MetaData::TypeSignature::ExtractTypeRef( mdTokenSet& set )
{
    SetReference( set, m_token );

    if(m_sub) m_sub->ExtractTypeRef( set );
}

//--//

bool MetaData::TypeSignature::operator==( const TypeSignature& sig ) const
{
    if(m_opt       != sig.m_opt      ) return false;
    if(m_rank      != sig.m_rank     ) return false;
    if(m_sizes     != sig.m_sizes    ) return false;
    if(m_lowBounds != sig.m_lowBounds) return false;

    if(m_token != mdTokenNil)
    {
        Parser*  prLeft;
        Parser*  prRight;
        TypeDef* tdLeft;
        TypeDef* tdRight;

        if(sig.m_token == mdTokenNil) return false;

        if(FAILED(    m_holder->m_holder->ResolveTypeDef(     m_holder,     m_token, prLeft , tdLeft  ))) return false;
        if(FAILED(sig.m_holder->m_holder->ResolveTypeDef( sig.m_holder, sig.m_token, prRight, tdRight ))) return false;

        if(tdLeft != tdRight) return false;
    }
    else
    {
        if(sig.m_token != mdTokenNil) return false;
    }

    if(m_sub)
    {
        if(sig.m_sub == NULL) return false;

        if(*m_sub != *sig.m_sub) return false;
    }
    else
    {
        if(sig.m_sub != NULL) return false;
    }

    return true;
}

//--//

HRESULT MetaData::TypeSignature::Parse( PCCOR_SIGNATURE& pSigBlob )
{
    while(true)
    {
        m_opt = CorSigUncompressElementType( pSigBlob );

        switch(m_opt)
        {
            case ELEMENT_TYPE_VOID      : return S_OK;
            case ELEMENT_TYPE_BOOLEAN   : return S_OK;
            case ELEMENT_TYPE_CHAR      : return S_OK;
            case ELEMENT_TYPE_I1        : return S_OK;
            case ELEMENT_TYPE_U1        : return S_OK;
            case ELEMENT_TYPE_I2        : return S_OK;
            case ELEMENT_TYPE_U2        : return S_OK;
            case ELEMENT_TYPE_I4        : return S_OK;
            case ELEMENT_TYPE_U4        : return S_OK;
            case ELEMENT_TYPE_I8        : return S_OK;
            case ELEMENT_TYPE_U8        : return S_OK;
            case ELEMENT_TYPE_R4        : return S_OK;
            case ELEMENT_TYPE_R8        : return S_OK;
            case ELEMENT_TYPE_STRING    : return S_OK;
            case ELEMENT_TYPE_PTR       : return ParseSubType( pSigBlob );
            case ELEMENT_TYPE_BYREF     : return ParseSubType( pSigBlob );
            case ELEMENT_TYPE_VALUETYPE : return ParseToken  ( pSigBlob );
            case ELEMENT_TYPE_CLASS     : return ParseToken  ( pSigBlob );
            case ELEMENT_TYPE_ARRAY     : return ParseArray  ( pSigBlob );
            case ELEMENT_TYPE_TYPEDBYREF: return S_OK;
            case ELEMENT_TYPE_I         : return S_OK;
            case ELEMENT_TYPE_U         : return S_OK;
            case ELEMENT_TYPE_FNPTR     : return ParseToken  ( pSigBlob );
            case ELEMENT_TYPE_OBJECT    : return S_OK;
            case ELEMENT_TYPE_SZARRAY   : return ParseSzArray( pSigBlob );

            // If ELEMENT_TYPE_PINNED was read - means it is type modifier. 
            // We move it to type modifier and clear the type. 
            // The type will be read by next call to CorSigUncompressElementType
            case ELEMENT_TYPE_PINNED:
                m_optTypeModifier = m_opt;
                m_opt = ELEMENT_TYPE_END;
            // Continue to read type of pinned variable.
            break;
            
            //
            // Skip unsupported C modifiers.
            //
            case ELEMENT_TYPE_CMOD_REQD:
            case ELEMENT_TYPE_CMOD_OPT :
                (void)CorSigUncompressToken( pSigBlob );
                break;



            case ELEMENT_TYPE_VAR:
            case ELEMENT_TYPE_MVAR:
            case ELEMENT_TYPE_GENERICINST:
                return CLR_E_PARSER_UNSUPPORTED_GENERICS;                
            default:
                return CLR_E_NOTIMPL;
        }
    }
}

HRESULT MetaData::TypeSignature::ParseToken( PCCOR_SIGNATURE& pSigBlob )
{
    TINYCLR_HEADER();

    m_token = CorSigUncompressToken( pSigBlob );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT MetaData::TypeSignature::ParseSubType( PCCOR_SIGNATURE& pSigBlob )
{
    TINYCLR_HEADER();

    m_sub = new TypeSignature( m_holder );

    TINYCLR_SET_AND_LEAVE(m_sub->Parse( pSigBlob ));

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::TypeSignature::ParseSzArray( PCCOR_SIGNATURE& pSigBlob )
{
    TINYCLR_HEADER();

    m_rank = 1;

    TINYCLR_SET_AND_LEAVE(ParseSubType( pSigBlob ));

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::TypeSignature::ParseArray( PCCOR_SIGNATURE& pSigBlob )
{
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT(ParseSubType( pSigBlob ));

    m_rank       = CorSigUncompressData( pSigBlob );
    int numSizes = CorSigUncompressData( pSigBlob );
    while(numSizes-- > 0)
    {
        m_sizes.push_back( CorSigUncompressData( pSigBlob ) );
    }

    int numLoBounds = CorSigUncompressData( pSigBlob );
    while(numLoBounds-- > 0)
    {
        m_lowBounds.push_back( CorSigUncompressData( pSigBlob ) );
    }

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////

#define PARSESIGNATURE_CHECKRANGE(sig,pos) if(pos >= sig.size()) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL)
#define PARSESIGNATURE_SCAN(sig,pos,fmt,val) if(swscanf_s( sig[pos++].c_str(), fmt, &val ) != 1) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL)

#define PARSESIGNATURE_CLEANUP(hr,sig) TINYCLR_CLEANUP(); if(FAILED(hr) && hr != CLR_E_PARSER_BAD_TEXT_SIGNATURE) { hr = CLR_E_PARSER_BAD_TEXT_SIGNATURE; DumpSignature( sig ); } TINYCLR_CLEANUP_END()

static void DumpSignature( CLR_RT_StringVector& sig )
{
    wprintf( L"Invalid signature:\n" );
    for(size_t i=0; i<sig.size(); i++)
    {
        wprintf( L" %s", sig[i].c_str() );
    }
    wprintf( L"\n" );
}

HRESULT MetaData::TypeSignature::Parse( CLR_RT_StringVector& sig, CLR_RT_StringVector::size_type& pos )
{
    TINYCLR_HEADER();

    LPCWSTR val;

    while(true)
    {
        PARSESIGNATURE_CHECKRANGE(sig,pos);

        val = sig[pos++].c_str();

        if     (!_wcsicmp( val, L"VOID"       )) { m_opt = ELEMENT_TYPE_VOID      ; }
        else if(!_wcsicmp( val, L"BOOLEAN"    )) { m_opt = ELEMENT_TYPE_BOOLEAN   ; }
        else if(!_wcsicmp( val, L"CHAR"       )) { m_opt = ELEMENT_TYPE_CHAR      ; }
        else if(!_wcsicmp( val, L"I1"         )) { m_opt = ELEMENT_TYPE_I1        ; }
        else if(!_wcsicmp( val, L"U1"         )) { m_opt = ELEMENT_TYPE_U1        ; }
        else if(!_wcsicmp( val, L"I2"         )) { m_opt = ELEMENT_TYPE_I2        ; }
        else if(!_wcsicmp( val, L"U2"         )) { m_opt = ELEMENT_TYPE_U2        ; }
        else if(!_wcsicmp( val, L"I4"         )) { m_opt = ELEMENT_TYPE_I4        ; }
        else if(!_wcsicmp( val, L"U4"         )) { m_opt = ELEMENT_TYPE_U4        ; }
        else if(!_wcsicmp( val, L"I8"         )) { m_opt = ELEMENT_TYPE_I8        ; }
        else if(!_wcsicmp( val, L"U8"         )) { m_opt = ELEMENT_TYPE_U8        ; }
        else if(!_wcsicmp( val, L"R4"         )) { m_opt = ELEMENT_TYPE_R4        ; }
        else if(!_wcsicmp( val, L"R8"         )) { m_opt = ELEMENT_TYPE_R8        ; }
        else if(!_wcsicmp( val, L"STRING"     )) { m_opt = ELEMENT_TYPE_STRING    ; }
        else if(!_wcsicmp( val, L"PTR"        )) { m_opt = ELEMENT_TYPE_PTR       ; }
        else if(!_wcsicmp( val, L"BYREF"      )) { m_opt = ELEMENT_TYPE_BYREF     ; }
        else if(!_wcsicmp( val, L"VALUETYPE"  )) { m_opt = ELEMENT_TYPE_VALUETYPE ; }
        else if(!_wcsicmp( val, L"CLASS"      )) { m_opt = ELEMENT_TYPE_CLASS     ; }
        else if(!_wcsicmp( val, L"ARRAY"      )) { m_opt = ELEMENT_TYPE_ARRAY     ; }
        else if(!_wcsicmp( val, L"TYPEDBYREF" )) { m_opt = ELEMENT_TYPE_TYPEDBYREF; }
        else if(!_wcsicmp( val, L"I"          )) { m_opt = ELEMENT_TYPE_I         ; }
        else if(!_wcsicmp( val, L"U"          )) { m_opt = ELEMENT_TYPE_U         ; }
        else if(!_wcsicmp( val, L"FNPTR"      )) { m_opt = ELEMENT_TYPE_FNPTR     ; }
        else if(!_wcsicmp( val, L"OBJECT"     )) { m_opt = ELEMENT_TYPE_OBJECT    ; }
        else if(!_wcsicmp( val, L"SZARRAY"    )) { m_opt = ELEMENT_TYPE_SZARRAY   ; }
        else                                     { TINYCLR_SET_AND_LEAVE(CLR_E_NOTIMPL); }

        switch(m_opt)
        {
            case ELEMENT_TYPE_VOID      : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_BOOLEAN   : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_CHAR      : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_I1        : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_U1        : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_I2        : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_U2        : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_I4        : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_U4        : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_I8        : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_U8        : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_R4        : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_R8        : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_STRING    : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_PTR       : TINYCLR_SET_AND_LEAVE(ParseSubType( sig, pos ));
            case ELEMENT_TYPE_BYREF     : TINYCLR_SET_AND_LEAVE(ParseSubType( sig, pos ));
            case ELEMENT_TYPE_VALUETYPE : TINYCLR_SET_AND_LEAVE(ParseToken  ( sig, pos ));
            case ELEMENT_TYPE_CLASS     : TINYCLR_SET_AND_LEAVE(ParseToken  ( sig, pos ));
            case ELEMENT_TYPE_ARRAY     : TINYCLR_SET_AND_LEAVE(ParseArray  ( sig, pos ));
            case ELEMENT_TYPE_TYPEDBYREF: TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_I         : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_U         : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_FNPTR     : TINYCLR_SET_AND_LEAVE(ParseToken  ( sig, pos ));
            case ELEMENT_TYPE_OBJECT    : TINYCLR_SET_AND_LEAVE(S_OK                    );
            case ELEMENT_TYPE_SZARRAY   : TINYCLR_SET_AND_LEAVE(ParseSzArray( sig, pos ));
        }
    }

    PARSESIGNATURE_CLEANUP(hr,sig);
}

HRESULT MetaData::TypeSignature::ParseToken( CLR_RT_StringVector& sig, CLR_RT_StringVector::size_type& pos )
{
    TINYCLR_HEADER();

    int val;

    PARSESIGNATURE_CHECKRANGE(sig,pos);

    PARSESIGNATURE_SCAN(sig,pos,L"0x%08x",val);

    m_token = val;

    PARSESIGNATURE_CLEANUP(hr,sig);
}

HRESULT MetaData::TypeSignature::ParseSubType( CLR_RT_StringVector& sig, CLR_RT_StringVector::size_type& pos )
{
    TINYCLR_HEADER();

    m_sub = new TypeSignature( m_holder );

    TINYCLR_SET_AND_LEAVE(m_sub->Parse( sig, pos ));

    PARSESIGNATURE_CLEANUP(hr,sig);
}

HRESULT MetaData::TypeSignature::ParseSzArray( CLR_RT_StringVector& sig, CLR_RT_StringVector::size_type& pos )
{
    TINYCLR_HEADER();

    m_rank = 1;

    TINYCLR_SET_AND_LEAVE(ParseSubType( sig, pos ));

    PARSESIGNATURE_CLEANUP(hr,sig);
}

HRESULT MetaData::TypeSignature::ParseArray( CLR_RT_StringVector& sig, CLR_RT_StringVector::size_type& pos )
{
    TINYCLR_HEADER();

    int val;

    TINYCLR_CHECK_HRESULT(ParseSubType( sig, pos ));

    PARSESIGNATURE_CHECKRANGE(sig,pos);
    PARSESIGNATURE_SCAN(sig,pos,L"%d",val);

    m_rank = val;


    PARSESIGNATURE_CHECKRANGE(sig,pos);
    PARSESIGNATURE_SCAN(sig,pos,L"%d",val);

    int numSizes = val;
    while(numSizes-- > 0)
    {
        PARSESIGNATURE_CHECKRANGE(sig,pos);
        PARSESIGNATURE_SCAN(sig,pos,L"%d",val);

        m_sizes.push_back( val );
    }


    PARSESIGNATURE_CHECKRANGE(sig,pos);
    PARSESIGNATURE_SCAN(sig,pos,L"%d",val);

    int numLoBounds = val;
    while(numLoBounds-- > 0)
    {
        PARSESIGNATURE_CHECKRANGE(sig,pos);
        PARSESIGNATURE_SCAN(sig,pos,L"%d",val);

        m_lowBounds.push_back( val );
    }

    PARSESIGNATURE_CLEANUP(hr,sig);
}

////////////////////////////////////////////////////////////////////////////////////////////////////

MetaData::MethodSignature::MethodSignature( Parser* holder ) : m_retValue(holder)
{
    m_holder = holder; // Parser*           m_holder;
                       //
    m_flags  = 0;      // BYTE              m_flags;
                       // TypeSignature     m_retValue;
                       // TypeSignatureList m_lstParams;
}

void MetaData::MethodSignature::ExtractTypeRef( mdTokenSet& set )
{
    m_retValue.ExtractTypeRef( set );

    for(TypeSignatureIter it = m_lstParams.begin(); it != m_lstParams.end(); it++)
    {
        it->ExtractTypeRef( set );
    }
}

//--//

HRESULT MetaData::MethodSignature::Parse( PCCOR_SIGNATURE& pSigBlob )
{
    TINYCLR_HEADER();

    m_flags = *pSigBlob++;

    if(m_flags & IMAGE_CEE_CS_CALLCONV_GENERIC) 
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_PARSER_UNSUPPORTED_GENERICS);
    }

    ULONG count = CorSigUncompressData( pSigBlob );

    TINYCLR_CHECK_HRESULT(m_retValue.Parse( pSigBlob ));

    for(ULONG i=0; i<count; i++)
    {
        TypeSignature param( m_holder );

        TINYCLR_CHECK_HRESULT(param.Parse( pSigBlob ));

        m_lstParams.push_back( param );
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT MetaData::MethodSignature::Parse( CLR_RT_StringVector& sig, CLR_RT_StringVector::size_type& pos )
{
    TINYCLR_HEADER();

    int val;

    PARSESIGNATURE_CHECKRANGE(sig,pos);
    PARSESIGNATURE_SCAN(sig,pos,L"0x%02x",val);

    m_flags = val;

    PARSESIGNATURE_CHECKRANGE(sig,pos);
    PARSESIGNATURE_SCAN(sig,pos,L"%d",val);

    ULONG count = val;

    TINYCLR_CHECK_HRESULT(m_retValue.Parse( sig, pos ));

    for(ULONG i=0; i<count; i++)
    {
        TypeSignature param( m_holder );

        TINYCLR_CHECK_HRESULT(param.Parse( sig, pos ));

        m_lstParams.push_back( param );
    }

    PARSESIGNATURE_CLEANUP(hr,sig);
}

bool MetaData::MethodSignature::operator==( const MethodSignature& sig ) const
{
    if(m_flags     != sig.m_flags    ) return false;
    if(m_retValue  != sig.m_retValue ) return false;
    if(m_lstParams != sig.m_lstParams) return false;

    return true;
}


////////////////////////////////////////////////////////////////////////////////////////////////////

MetaData::LocalVarSignature::LocalVarSignature( Parser* holder )
{
    m_holder = holder; // Parser*           m_holder;
                       //
                       // TypeSignatureList m_lstVars;
}

void MetaData::LocalVarSignature::ExtractTypeRef( mdTokenSet& set )
{
    for(TypeSignatureIter it = m_lstVars.begin(); it != m_lstVars.end(); it++)
    {
        it->ExtractTypeRef( set );
    }
}

//--//

HRESULT MetaData::LocalVarSignature::Parse( PCCOR_SIGNATURE& pSigBlob )
{
    TINYCLR_HEADER();

    if(*pSigBlob++ != IMAGE_CEE_CS_CALLCONV_LOCAL_SIG) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    ULONG count = CorSigUncompressData( pSigBlob );

    for(ULONG i=0; i<count; i++)
    {
        TypeSignature var( m_holder );

        TINYCLR_CHECK_HRESULT(var.Parse( pSigBlob ));

        m_lstVars.push_back( var );
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT MetaData::LocalVarSignature::Parse( CLR_RT_StringVector& sig, CLR_RT_StringVector::size_type& pos )
{
    TINYCLR_HEADER();

    int val;

    PARSESIGNATURE_CHECKRANGE(sig,pos);
    PARSESIGNATURE_SCAN(sig,pos,L"0x%02x",val);

    if(val != IMAGE_CEE_CS_CALLCONV_LOCAL_SIG) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    PARSESIGNATURE_CHECKRANGE(sig,pos);
    PARSESIGNATURE_SCAN(sig,pos,L"%d",val);

    ULONG count = val;

    for(ULONG i=0; i<count; i++)
    {
        TypeSignature var( m_holder );

        TINYCLR_CHECK_HRESULT(var.Parse( sig, pos ));

        m_lstVars.push_back( var );
    }

    PARSESIGNATURE_CLEANUP(hr,sig);
}

////////////////////////////////////////////////////////////////////////////////////////////////////

MetaData::TypeSpecSignature::TypeSpecSignature( Parser* holder ) : m_sigField(holder), m_sigLocal(holder), m_sigMethod(holder)
{
    m_holder = holder;        // Parser*           m_holder;
                              //
    m_type   = mdTokenNil;    // mdToken           m_type;
                              // TypeSignature     m_sigField;
                              // LocalVarSignature m_sigLocal;
                              // MethodSignature   m_sigMethod;
}
//
//MetaData::TypeSpecSignature::TypeSpecSignature( const TypeSpecSignature& tss ) : m_sigField(tss.m_sigField), m_sigLocal(tss.m_sigLocal), m_sigMethod(tss.m_sigMethod)
//{
//    m_holder = tss.m_holder;  // Parser*           m_holder;
//                              //
//    m_type   = tss.m_type;    // mdToken           m_type;
//                              // TypeSignature     m_sigField;
//                              // LocalVarSignature m_sigLocal;
//                              // MethodSignature   m_sigMethod;
//}
//
//MetaData::TypeSpecSignature& MetaData::TypeSpecSignature::operator= ( const TypeSpecSignature& tss )
//{
//    m_holder    = tss.m_holder;    // Parser*           m_holder;
//                                   //
//    m_type      = tss.m_type;      // mdToken           m_type;
//    m_sigField  = tss.m_sigField;  // TypeSignature     m_sigField;
//    m_sigLocal  = tss.m_sigLocal;  // LocalVarSignature m_sigLocal;
//    m_sigMethod = tss.m_sigMethod; // MethodSignature   m_sigMethod;
//
//    return *this;
//}

void MetaData::TypeSpecSignature::ExtractTypeRef( mdTokenSet& set )
{
    switch(m_type)
    {
    case mdtFieldDef : m_sigField .ExtractTypeRef( set ); break;
    case mdtSignature: m_sigLocal .ExtractTypeRef( set ); break;
    case mdtMethodDef: m_sigMethod.ExtractTypeRef( set ); break;
    }
}

//--//

HRESULT MetaData::TypeSpecSignature::Parse( PCCOR_SIGNATURE& pSigBlob )
{
    TINYCLR_HEADER();

    switch(*pSigBlob & IMAGE_CEE_CS_CALLCONV_MASK)
    {
    case IMAGE_CEE_CS_CALLCONV_FIELD:
        TINYCLR_CHECK_HRESULT(m_sigField.Parse( ++pSigBlob ));

        m_type = mdtFieldDef;
        break;

    case IMAGE_CEE_CS_CALLCONV_LOCAL_SIG:
        TINYCLR_CHECK_HRESULT(m_sigLocal.Parse( pSigBlob ));

        m_type = mdtSignature;
        break;

    case IMAGE_CEE_CS_CALLCONV_DEFAULT: // A method.
        TINYCLR_CHECK_HRESULT(m_sigMethod.Parse( pSigBlob ));

        m_type = mdtMethodDef;
        break;

    default:
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT MetaData::TypeSpecSignature::Parse( CLR_RT_StringVector& sig, CLR_RT_StringVector::size_type& pos )
{
    TINYCLR_HEADER();

    int val;

    PARSESIGNATURE_CHECKRANGE(sig,pos);
    PARSESIGNATURE_SCAN(sig,pos,L"0x%02x",val);

    switch(val & IMAGE_CEE_CS_CALLCONV_MASK)
    {
    case IMAGE_CEE_CS_CALLCONV_FIELD:
        TINYCLR_CHECK_HRESULT(m_sigField.Parse( sig, pos ));

        m_type = mdtFieldDef;
        break;

    case IMAGE_CEE_CS_CALLCONV_LOCAL_SIG:
        TINYCLR_CHECK_HRESULT(m_sigLocal.Parse( sig, pos ));

        m_type = mdtSignature;
        break;

    case IMAGE_CEE_CS_CALLCONV_DEFAULT: // A method.
        TINYCLR_CHECK_HRESULT(m_sigMethod.Parse( sig, pos ));

        m_type = mdtMethodDef;
        break;

    default:
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    PARSESIGNATURE_CLEANUP(hr,sig);
}

////////////////////////////////////////////////////////////////////////////////////////////////////

MetaData::AssemblyRef::AssemblyRef()
{
    m_ar    = mdAssemblyRefNil; // mdAssemblyRef      m_ar;
    m_flags = 0;                // DWORD              m_flags;
                                // std::wstring       m_name;
    TINYCLR_CLEAR(m_version);   // CLR_RECORD_VERSION m_version;
}

MetaData::ModuleRef::ModuleRef()
{
    m_mr = mdModuleRefNil; // mdModuleRef  m_mr;
                           // std::wstring m_name;
}

MetaData::TypeRef::TypeRef()
{
    m_tr    = mdTypeRefNil; // mdTypeRef    m_tr;
                            // std::wstring m_name;
    m_scope = mdTokenNil;   // mdToken      m_scope; // ResolutionScope coded index.
}

MetaData::MemberRef::MemberRef( Parser* holder ) : m_sig(holder)
{
    m_tr = mdTokenNil; // mdToken           m_tr; // MemberRefParent coded index.
    m_mr = mdTokenNil; // mdToken           m_mr; // MemberRefParent coded index.
                       // std::wstring      m_name;
                       // TypeSpecSignature m_sig;
}

MetaData::TypeDef::TypeDef()
{
    m_td             = mdTypeDefNil; // mdTypeDef           m_td;
    m_flags          = 0;            // DWORD               m_flags;
                                     // std::wstring        m_name;
    m_extends        = mdTokenNil;   // mdToken             m_extends; // TypeDefOrRef coded index.
    m_enclosingClass = mdTypeDefNil; // mdTypeDef           m_enclosingClass;
                                     //
                                     // mdFieldDefList      m_fields;
                                     // mdMethodDefList     m_methods;
                                     // mdInterfaceImplList m_interfaces;
}

MetaData::FieldDef::FieldDef( Parser* holder ) : m_sig(holder)
{
    m_td    = mdTypeDefNil;  // mdTypeDef     m_td;
    m_fd    = mdFieldDefNil; // mdFieldDef    m_fd;
    m_attr  = 0;             // DWORD         m_attr;
    m_flags = 0;             // DWORD         m_flags;
                             // std::wstring  m_name;
                             // TypeSignature m_sig;
}

bool MetaData::FieldDef::SetValue( const void* ptr, int len )
{
    if(len < 0 || len > 0x7FFF) return false;

    m_value.resize( len );

    memcpy( &m_value[0], ptr, len );

    return true;
}

MetaData::MethodDef::MethodDef( Parser* holder ) : m_method(holder), m_vars(holder)
{
    m_td        = mdTypeDefNil;   // mdTypeDef         m_td;
    m_md        = mdMethodDefNil; // mdMethodDef       m_md;
    m_implFlags = 0;              // DWORD             m_implFlags;
    m_flags     = 0;              // DWORD             m_flags;
                                  // MethodSignature   m_method;
                                  // LocalVarSignature m_vars;
    m_RVA       = 0;              // DWORD             m_RVA;
    m_VA        = NULL;           // const BYTE*       m_VA;
                                  // ByteCode          m_byteCode;
                                  // ByteCode          m_byteCodeOriginal;
    m_maxStack  = 0;              // CLR_UINT32        m_maxStack;

}

MetaData::InterfaceImpl::InterfaceImpl()
{
    m_td  = mdTypeDefNil; // mdTypeDef m_td;
    m_itf = mdTokenNil;   // mdToken   m_itf; // TypeDefOrRef
}

MetaData::TypeSpec::TypeSpec( Parser* holder ) : m_sig(holder)
{
    m_ts = mdTypeSpecNil; // mdTypeSpec    m_ts;
                          // TypeSignature m_sig;
}

MetaData::ManifestResource::ManifestResource( Parser* holder )
{
    m_mr               = mdManifestResourceNil; // mdManifestResource m_mr;
                                                // std::vector<BYTE>  m_blob;
    m_tkImplementation = mdTokenNil;            // mdToken            m_tkImplementation;
    m_dwResourceFlags  = 0;                     // DWORD              m_dwResourceFlags;
                                                //
    m_fUsed            = false;                 // bool               m_fUsed;
}

//--//

MetaData::CustomAttribute::Reader::Reader( std::vector<BYTE>& blob ) : m_blob( blob ), m_pos( &blob[0] )
{
}

bool MetaData::CustomAttribute::Reader::Read( void* dst, int size )
{
    if(m_pos + size > &m_blob[0]+m_blob.size()) return false;

    memcpy( dst, m_pos, size ); m_pos += size;

    return true;
}

bool MetaData::CustomAttribute::Reader::ReadCompressedLength( int& val )
{
    CLR_UINT8 b[4];
    int       size;

    if(!Read( b[0] )) return false;

    if(b[0] != 0xFF)
    {
        if((b[0] & 0x80) == 0)
        {
            // encoded as a single byte

            size = b[0];
        }
        else if((b[0] & 0xC0) == 0x80)
        {
            // encoded in two bytes
            if(!Read( b[1] )) return false;

            size  = (int)(b[0] & ~0xC0) << 8;
            size |= (int) b[1]             ; // This is in big-endian format
        }
        else
        {
            // encoded in four bytes
            if(!Read( b[1] )) return false;
            if(!Read( b[2] )) return false;
            if(!Read( b[3] )) return false;

            size  = (int)(b[0] & ~0xC0) << 24;
            size |= (int) b[1]          << 16;
            size |= (int) b[2]          <<  8;
            size |= (int) b[3]               ;
        }
    }
    else
    {
        size = -1;
    }

    val = size;

    return true;
}

bool MetaData::CustomAttribute::Reader::Read( CorSerializationType& val )
{
    BYTE tmp = 0;
    bool res = Read( &tmp, sizeof(tmp) );

    val = (CorSerializationType)tmp;

    return res;
}


bool MetaData::CustomAttribute::Reader::Read( CLR_UINT8& val )
{
    return Read( &val, sizeof(val) );
}

bool MetaData::CustomAttribute::Reader::Read( CLR_UINT16& val )
{
    return Read( &val, sizeof(val) );
}

bool MetaData::CustomAttribute::Reader::Read( CLR_UINT32& val )
{
    return Read( &val, sizeof(val) );
}

bool MetaData::CustomAttribute::Reader::Read( CLR_UINT64& val )
{
    return Read( &val, sizeof(val) );
}

bool MetaData::CustomAttribute::Reader::Read( std::wstring& val )
{
    int         size;
    std::string tmp;

    if(!ReadCompressedLength( size )) return false;

    tmp.resize( size );
    LPSTR ptr = (LPSTR)tmp.c_str();

    if(!Read( ptr, size )) return false;
    ptr[size] = 0;

    CLR_RT_UnicodeHelper::ConvertFromUTF8( tmp, val );

    return true;
}

//--//

MetaData::CustomAttribute::Writer::Writer( WatchAssemblyBuilder::Linker* lk, BYTE*& ptr, BYTE* end ) : m_lk( lk ), m_ptr( ptr ), m_end( end )
{
}

bool MetaData::CustomAttribute::Writer::Write( const void* src, int size )
{
    if(m_ptr + size > m_end) return false;

    memcpy( m_ptr, src, size ); m_ptr += size;

    return true;
}

bool MetaData::CustomAttribute::Writer::WriteCompressedLength( int val )
{
    CLR_UINT8 b[4];
    int       len;

    if(val == -1)
    {
        b[0] = 0xFF;
        len  = 1;
    }
    else if(val < 0x80)
    {
        b[0] = (CLR_UINT8)val;
        len  = 1;
    }
    else if(val < ~0xC000)
    {
        b[0] = (CLR_UINT8)(0x80 | ((val >> 8) & ~0xC0));
        b[1] = (CLR_UINT8)(       ((val     ) &  0xFF));
        len  = 2;
    }
    else
    {
        b[0] = (CLR_UINT8)(0xC0 | ((val >> 24) & ~0xC0));
        b[1] = (CLR_UINT8)(       ((val >> 16) &  0xFF));
        b[2] = (CLR_UINT8)(       ((val >>  8) &  0xFF));
        b[3] = (CLR_UINT8)(       ((val      ) &  0xFF));
        len  = 4;
    }

    return Write( b, len );
}

bool MetaData::CustomAttribute::Writer::Write( const CorSerializationType val )
{
    BYTE tmp = val;

    return Write( &tmp, sizeof(tmp) );
}


bool MetaData::CustomAttribute::Writer::Write( const CLR_UINT8 val )
{
    return Write( &val, sizeof(val) );
}

bool MetaData::CustomAttribute::Writer::Write( const CLR_UINT16 val )
{
    return Write( &val, sizeof(val) );
}

bool MetaData::CustomAttribute::Writer::Write( const CLR_UINT32 val )
{
    return Write( &val, sizeof(val) );
}

bool MetaData::CustomAttribute::Writer::Write( const CLR_UINT64 val )
{
    return Write( &val, sizeof(val) );
}

bool MetaData::CustomAttribute::Writer::Write( const std::wstring& val )
{
    CLR_STRING idx;

    if(!m_lk->AllocString( val, idx, false )) return false;

    return Write( idx );
}

//--//

bool MetaData::CustomAttribute::Value::Parse( Reader& reader )
{
    switch(m_opt)
    {
    case SERIALIZATION_TYPE_BOOLEAN: return reader.Read( m_numeric.u1 );
    case SERIALIZATION_TYPE_CHAR   : return reader.Read( m_numeric.u2 );
    case SERIALIZATION_TYPE_I1     : return reader.Read( m_numeric.u1 );
    case SERIALIZATION_TYPE_U1     : return reader.Read( m_numeric.u1 );
    case SERIALIZATION_TYPE_I2     : return reader.Read( m_numeric.u2 );
    case SERIALIZATION_TYPE_U2     : return reader.Read( m_numeric.u2 );
    case SERIALIZATION_TYPE_I4     : return reader.Read( m_numeric.u4 );
    case SERIALIZATION_TYPE_U4     : return reader.Read( m_numeric.u4 );
    case SERIALIZATION_TYPE_I8     : return reader.Read( m_numeric.u8 );
    case SERIALIZATION_TYPE_U8     : return reader.Read( m_numeric.u8 );
    case SERIALIZATION_TYPE_R4     : return reader.Read( m_numeric.u4 );
    case SERIALIZATION_TYPE_R8     : return reader.Read( m_numeric.u8 );
    case SERIALIZATION_TYPE_STRING : return reader.Read( m_text       );
    case SERIALIZATION_TYPE_TYPE   : return reader.Read( m_text       );
    case SERIALIZATION_TYPE_ENUM   : return reader.Read( m_numeric.u4 );
    }

    wprintf( L"Wrong type in blob: %02x\n", m_opt );
    return false;
}

bool MetaData::CustomAttribute::Value::Emit( Writer& writer )
{
    if(writer.Write( (CLR_UINT8)m_opt ) == false) return false;

    switch(m_opt)
    {
    case SERIALIZATION_TYPE_BOOLEAN: return writer.Write( m_numeric.u1 );
    case SERIALIZATION_TYPE_CHAR   : return writer.Write( m_numeric.u2 );
    case SERIALIZATION_TYPE_I1     : return writer.Write( m_numeric.u1 );
    case SERIALIZATION_TYPE_U1     : return writer.Write( m_numeric.u1 );
    case SERIALIZATION_TYPE_I2     : return writer.Write( m_numeric.u2 );
    case SERIALIZATION_TYPE_U2     : return writer.Write( m_numeric.u2 );
    case SERIALIZATION_TYPE_I4     : return writer.Write( m_numeric.u4 );
    case SERIALIZATION_TYPE_U4     : return writer.Write( m_numeric.u4 );
    case SERIALIZATION_TYPE_I8     : return writer.Write( m_numeric.u8 );
    case SERIALIZATION_TYPE_U8     : return writer.Write( m_numeric.u8 );
    case SERIALIZATION_TYPE_R4     : return writer.Write( m_numeric.u4 );
    case SERIALIZATION_TYPE_R8     : return writer.Write( m_numeric.u8 );
    case SERIALIZATION_TYPE_STRING : return writer.Write( m_text       );
    case SERIALIZATION_TYPE_TYPE   : return writer.Write( m_text       );
    case SERIALIZATION_TYPE_ENUM   : return writer.Write( m_numeric.u4 );
    }

    wprintf( L"Wrong type in blob: %02x\n", m_opt );
    return false;
}

MetaData::CustomAttribute::CustomAttribute( Parser* holder )
{
    m_holder = holder;     // Parser*           m_holder;
                           //
    m_tkObj  = mdTokenNil; // mdToken           m_tkObj;
    m_tkType = mdTokenNil; // mdToken           m_tkType;
                           // std::vector<BYTE> m_blob;
                           // ValueList         m_valuesFixed;
                           // ValueMap          m_valuesVariable;

}

HRESULT MetaData::CustomAttribute::Parse()
{
    TINYCLR_HEADER();

    Parser*    prDst;
    MethodDef* mdDst;

    TINYCLR_CHECK_HRESULT(m_holder->m_holder->ResolveMethodDef( m_holder, m_tkType, prDst, mdDst ));

    {
        Parser*  prDst2;
        TypeDef* tdDst2;

        TINYCLR_CHECK_HRESULT(m_holder->m_holder->ResolveTypeDef( prDst, mdDst->m_td, prDst2, tdDst2 ));

        m_nameOfAttributeClass = tdDst2->m_name.c_str();
    }

    {
        CustomAttribute::Reader reader( m_blob );
        CLR_UINT16              val;
        CorSerializationType    et;

        if(!reader.Read( val )) goto InvalidBlob;

        if(val != 0x0001)
        {
            wprintf( L"Wrong signature for CustomAttribute blob: %04x\n", val );
            TINYCLR_SET_AND_LEAVE(CLR_E_PARSER_BAD_CUSTOM_ATTRIBUTE);
        }

        for(TypeSignatureIter itTS = mdDst->m_method.m_lstParams.begin(); itTS != mdDst->m_method.m_lstParams.end(); itTS++)
        {
            CustomAttribute::Value v;
            TypeSignature&         sig   = *itTS;
            bool                   fEnum = false;

            if(sig.m_opt == ELEMENT_TYPE_VALUETYPE)
            {
                Parser*  prDst2;
                TypeDef* tdDst2;

                TINYCLR_CHECK_HRESULT(m_holder->m_holder->ResolveTypeDef( sig.m_holder, sig.m_token, prDst2, tdDst2 ));

                if(IsNilToken(tdDst2->m_extends) == false)
                {
                    std::wstring* name;

                    if(TypeFromToken( tdDst2->m_extends ) == mdtTypeDef)
                    {
                        name = &prDst2->m_mapDef_Type.find(tdDst2->m_extends)->second.m_name;
                    }
                    else
                    {
                        name = &prDst2->m_mapRef_Type[tdDst2->m_extends].m_name;
                    }

                    if(*name == L"System.Enum")
                    {
                        fEnum = true;
                    }
                }
            }

            v.m_opt = (CorSerializationType)(fEnum ? ELEMENT_TYPE_I4 : sig.m_opt);

            if(v.Parse( reader ) == false)
            {
                wprintf( L"Unable to read value %d: %02x\n", m_valuesFixed.size(), v.m_opt );
                goto InvalidBlob;
            }

            m_valuesFixed.push_back( v );
        }

        if(!reader.Read( val )) goto InvalidBlob;
        while(val)
        {
            CustomAttribute::Name  n;
            CustomAttribute::Value v;

            if(!reader.Read( et )) goto InvalidBlob;

            switch(et)
            {
            case SERIALIZATION_TYPE_FIELD:
                n.m_fField = true;
                break;

            case SERIALIZATION_TYPE_PROPERTY:
                n.m_fField = false;
                break;

            default:
                wprintf( L"Wrong type in blob: %02x\n", et );
                TINYCLR_SET_AND_LEAVE(CLR_E_PARSER_BAD_CUSTOM_ATTRIBUTE);
            }

            if(!reader.Read( v.m_opt  )) goto InvalidBlob;

            //
            // Skip type, in case the argument is of type Enum.
            //
            if(v.m_opt == SERIALIZATION_TYPE_ENUM)
            {
                std::wstring enumType;

                if(reader.Read( enumType ) == false) return false;
            }

            if(!reader.Read( n.m_text )) goto InvalidBlob;

            if(v.Parse( reader ) == false)
            {
                wprintf( L"Unable to read value %d: %02x\n", m_valuesVariable.size(), v.m_opt );
                goto InvalidBlob;
            }

            m_valuesVariable[n] = v;

            val--;
        }

        if(reader.m_pos != &m_blob[0]+m_blob.size()) goto InvalidBlob;

        TINYCLR_SET_AND_LEAVE(S_OK);

    InvalidBlob:
        wprintf( L"Invalid blob:\n\n" );

        TINYCLR_SET_AND_LEAVE(CLR_E_PARSER_BAD_CUSTOM_ATTRIBUTE);
    }

    TINYCLR_NOCLEANUP();
}


//--//

MetaData::Parser::Parser( Collection* holder )
{
    m_holder = holder;              // Collection*                      m_holder;
                                    //
                                    // std::wstring                     m_assemblyName;
                                    // std::wstring                     m_assemblyFile;
                                    //
                                    // AssemblyRefMap                   m_mapRef_Assembly;
                                    // ModuleRefMap                     m_mapRef_Module;
                                    // TypeRefMap                       m_mapRef_Type;
                                    // MemberRefMap                     m_mapRef_Member;
                                    //
                                    // TypeDefMap                       m_mapDef_Type;
                                    // FieldDefMap                      m_mapDef_Field;
                                    // MethodDefMap                     m_mapDef_Method;
                                    // InterfaceImplMap                 m_mapDef_Interface;
                                    //
                                    // CustomAttributeMap               m_mapDef_CustomAttribute;
                                    // mdTokenSet                       m_setAttributes_Methods_NativeProfiler;
                                    // mdTokenSet                       m_setAttributes_Methods_GloballySynchronized;
                                    // mdTokenSet                       m_setAttributes_Types_PublishInApplicationDirectory;
                                    //
                                    // TypeSpecMap                      m_mapSpec_Type;
                                    //
                                    // UserStringMap                    m_mapDef_String;
                                    // ManifestResourceMap              m_mapDef_ManifestResource;
                                    //
                                    //
                                    //
                                    //
    m_fVerboseMinimize     = false; // bool                             m_fVerboseMinimize;
                                    //
    m_fNoByteCode          = false; // bool                             m_fNoByteCode;
    m_fNoAttributes        = false; // bool                             m_fNoAttributes;
                                    // CLR_RT_StringSet                 m_setFilter_ExcludeClassByName;
                                    //
                                    // //--//
                                    //
                                    // CComPtr<IMetaDataDispenserEx>    m_pDisp;
                                    // CComPtr<IMetaDataImport>         m_pImport;
                                    // CComPtr<IMetaDataImport>         m_pImport2;
                                    // CComPtr<IMetaDataAssemblyImport> m_pAssemblyImport;
                                    // CComPtr<ISymUnmanagedReader>     m_pSymReader;
                                    // PELoader                         m_pe;
    m_output  = stdout;             // FILE*                            m_output;
    m_toclose = NULL;               // FILE*                            m_toclose;
    m_fSwapEndian = false;
}

MetaData::Parser::~Parser()
{
    Dump_CloseDevice();
}

//--//

#define HELPER_QUICKSTRING_DECL(buf,size)  CQuickWSTR buf; ULONG size; buf.Maximize();

#define HELPER_QUICKSTRING_CALL(buf,size)  buf.Ptr(), (ULONG)(buf.MaxSize()-1), &size
#define HELPER_QUICKSTRING_CHECK(buf,size) if(buf.MaxSize() > size) { buf[size] = 0; break; } TINYCLR_CHECK_HRESULT(buf.ReSizeNoThrow( size+1 ))

//--//

HRESULT MetaData::Parser::GetAssemblyDef()
{
    TINYCLR_HEADER();

    mdAssembly tkAsm;

    TINYCLR_CHECK_HRESULT(m_pAssemblyImport->GetAssemblyFromScope( &tkAsm ));

    if(tkAsm != mdAssemblyNil)
    {
        const void*      pPublicKey;
        ULONG            cbPublicKey = 0;
        ULONG            ulHashAlgId;
        WCHAR            wzName[1024];
        ULONG            ulNameLen = 0;
        ASSEMBLYMETADATA md; TINYCLR_CLEAR(md);
        WCHAR            wzLocale[1024];
        DWORD            dwFlags;

        md.szLocale = wzLocale;
        md.cbLocale = ARRAYSIZE(wzLocale);

        TINYCLR_CHECK_HRESULT(m_pAssemblyImport->GetAssemblyProps(  tkAsm            ,   // [IN ] The Assembly for which to get the properties.
                                                                   &pPublicKey       ,   // [OUT] Pointer to the public key.
                                                                   &cbPublicKey      ,   // [OUT] Count of bytes in the public key.
                                                                   &ulHashAlgId      ,   // [OUT] Hash Algorithm.
                                                                    wzName           ,   // [OUT] Buffer to fill with name.
                                                                    ARRAYSIZE(wzName),   // [IN ] Size of buffer in wide chars.
                                                                   &ulNameLen        ,   // [OUT] Actual # of wide chars in name.
                                                                   &md               ,   // [OUT] Assembly MetaData.
                                                                   &dwFlags          )); // [OUT] Flags.

        m_assemblyName            = wzName;
        m_version.iMajorVersion   = md.usMajorVersion;
        m_version.iMinorVersion   = md.usMinorVersion;
        m_version.iBuildNumber    = md.usBuildNumber;
        m_version.iRevisionNumber = md.usRevisionNumber;
        m_tkAsm                   = tkAsm;
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::Parser::GetAssemblyRef( mdAssemblyRef ar )
{
    TINYCLR_HEADER();

    if(m_mapRef_Assembly.find( ar ) == m_mapRef_Assembly.end())
    {
        AssemblyRef&     db = m_mapRef_Assembly[ar];
        ASSEMBLYMETADATA md; TINYCLR_CLEAR(md);

        HELPER_QUICKSTRING_DECL(buf,size);

        while(true)
        {
            TINYCLR_CHECK_HRESULT(m_pAssemblyImport->GetAssemblyRefProps( ar, NULL, NULL, HELPER_QUICKSTRING_CALL(buf,size), &md, NULL, NULL, &db.m_flags ));

            HELPER_QUICKSTRING_CHECK(buf,size);
        }

        db.m_ar                      = ar;
        db.m_name                    = buf.Ptr();
        db.m_version.iMajorVersion   = md.usMajorVersion;
        db.m_version.iMinorVersion   = md.usMinorVersion;
        db.m_version.iBuildNumber    = md.usBuildNumber;
        db.m_version.iRevisionNumber = md.usRevisionNumber;
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::Parser::GetModuleRef( mdModuleRef mr )
{
    TINYCLR_HEADER();

    if(m_mapRef_Module.find( mr ) == m_mapRef_Module.end())
    {
        ModuleRef& db = m_mapRef_Module[mr];

        HELPER_QUICKSTRING_DECL(buf,size);

        while(true)
        {
            TINYCLR_CHECK_HRESULT(m_pImport->GetModuleRefProps( mr, HELPER_QUICKSTRING_CALL(buf,size) ));

            HELPER_QUICKSTRING_CHECK(buf,size);
        }

        db.m_mr   = mr;
        db.m_name = buf.Ptr();
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::Parser::GetTypeRef( mdTypeRef tr )
{
    TINYCLR_HEADER();

    if(m_mapRef_Type.find( tr ) == m_mapRef_Type.end())
    {
        TypeRef& db = m_mapRef_Type[tr];

        HELPER_QUICKSTRING_DECL(buf,size);

        while(true)
        {
            TINYCLR_CHECK_HRESULT(m_pImport->GetTypeRefProps( tr, &db.m_scope, HELPER_QUICKSTRING_CALL(buf,size) ));

            HELPER_QUICKSTRING_CHECK(buf,size);

            if ( size > MAXTYPENAMELEN )
            {
                std::wstring str = L"<unknown>";
                // if(SUCCEEDED(ErrorReporting::ConstructErrorOrigin( str, m_pSymReader, md, 0 )))
                {
                    ErrorReporting::Print( str.c_str(), NULL, TRUE, 0, L"Length of name of type '%s' (%d) is longer than %d characters", buf.Ptr(), size, MAXTYPENAMELEN );
                }
                TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
            }
        }

        db.m_tr   = tr;
        db.m_name = buf.Ptr();

        TINYCLR_CHECK_HRESULT(EnumMemberRefs( db ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::Parser::GetMemberRef( mdMemberRef mr )
{
    TINYCLR_HEADER();

    if(m_mapRef_Member.find( mr ) == m_mapRef_Member.end())
    {
        MemberRef       db( this );
        PCCOR_SIGNATURE pSigBlob;
        ULONG           cbSigBlob;

        HELPER_QUICKSTRING_DECL(buf,size);

        while(true)
        {
            TINYCLR_CHECK_HRESULT(m_pImport->GetMemberRefProps( mr, &db.m_tr, HELPER_QUICKSTRING_CALL(buf,size), &pSigBlob, &cbSigBlob ));

            HELPER_QUICKSTRING_CHECK(buf,size);
        }

        db.m_mr   = mr;
        db.m_name = buf.Ptr();

        TINYCLR_CHECK_HRESULT(db.m_sig.Parse( pSigBlob ));

        m_mapRef_Member.insert( MemberRefMap::value_type( mr, db ) );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::Parser::GetTypeDef( mdTypeDef td )
{
    TINYCLR_HEADER();

    if(m_mapDef_Type.find( td ) == m_mapDef_Type.end())
    {
        TypeDef  db;

        HELPER_QUICKSTRING_DECL(buf,size);

        while(true)
        {
            TINYCLR_CHECK_HRESULT(m_pImport->GetTypeDefProps( td, HELPER_QUICKSTRING_CALL(buf,size), &db.m_flags, &db.m_extends ));

            HELPER_QUICKSTRING_CHECK(buf,size);

            if ( size > MAXTYPENAMELEN )
            {
                std::wstring str = L"<unknown>";
                // if(SUCCEEDED(ErrorReporting::ConstructErrorOrigin( str, m_pSymReader, md, 0 )))
                {
                    ErrorReporting::Print( str.c_str(), NULL, TRUE, 0, L"Length of name of type '%s' (%d) is longer than %d characters", buf.Ptr(), size, MAXTYPENAMELEN );
                }
                TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);
            }
        }

        if(IsTdNested(db.m_flags))
        {
            TINYCLR_CHECK_HRESULT(m_pImport->GetNestedClassProps( td, &db.m_enclosingClass ));
        }

        db.m_td   = td;
        db.m_name = buf.Ptr();
        m_mapDef_Type.insert( td, db );
    }

    TINYCLR_NOCLEANUP();
}

int MetaData::Parser::SizeFromElementType(CorElementType et)
{       
    int len;

    switch(et)
    {
        case ELEMENT_TYPE_BOOLEAN: len = 1; break;
        case ELEMENT_TYPE_CHAR   : len = 2; break;
        case ELEMENT_TYPE_I1     : len = 1; break;
        case ELEMENT_TYPE_U1     : len = 1; break;
        case ELEMENT_TYPE_I2     : len = 2; break;
        case ELEMENT_TYPE_U2     : len = 2; break;
        case ELEMENT_TYPE_I4     : len = 4; break;
        case ELEMENT_TYPE_U4     : len = 4; break;
        case ELEMENT_TYPE_I8     : len = 8; break;
        case ELEMENT_TYPE_U8     : len = 8; break;
        case ELEMENT_TYPE_R4     : len = 4; break;
        case ELEMENT_TYPE_R8     : len = 8; break;
        default                  : len = 0; break;
    }

    return len;
}

HRESULT MetaData::Parser::GetTypeField( mdFieldDef fd )
{
    TINYCLR_HEADER();

    if(m_mapDef_Field.find( fd ) == m_mapDef_Field.end())
    {
        FieldDef        db( this );
        PCCOR_SIGNATURE pSigBlob;
        ULONG           cbSigBlob;
        const void*     pValue;
        ULONG           chValue;
        int             len = 0;

        HELPER_QUICKSTRING_DECL(buf,size);

        while(true)
        {
            TINYCLR_CHECK_HRESULT(m_pImport->GetFieldProps( fd, &db.m_td, HELPER_QUICKSTRING_CALL(buf,size), &db.m_flags, &pSigBlob, &cbSigBlob, &db.m_attr, &pValue, &chValue ));

            HELPER_QUICKSTRING_CHECK(buf,size);
        }

        db.m_fd   = fd;
        db.m_name = buf.Ptr();
        
        TINYCLR_CHECK_HRESULT(db.m_sig.Parse( pSigBlob ));

        if(pValue)
        {
            if(db.m_attr == ELEMENT_TYPE_STRING)
            {
                len = chValue * sizeof(WCHAR);
            }
            // enable const object fields
            else if(db.m_attr == ELEMENT_TYPE_CLASS && IsFdLiteral(db.m_flags) && IsFdStatic(db.m_flags))
            {
                len = 4;
            }
            else
            {
                len = SizeFromElementType( (CorElementType)db.m_attr );
            }
        }
        
        if(IsFdHasFieldRVA( db.m_flags ))
        {
            //
            // Constant value code.
            //       
            ULONG methRVA;

            _ASSERTE(pValue == NULL);

            TINYCLR_CHECK_HRESULT( m_pImport->GetRVA( fd, &methRVA, NULL ));

            if(methRVA)
            {
                void* VA;

                if(m_pe.GetVAforRVA( methRVA, VA ))
                {
                    ULONG ulSize = 0;
                    CorElementType et = db.m_sig.m_sigField.m_opt;

                    if(et == ELEMENT_TYPE_VALUETYPE)
                    {
                        TINYCLR_CHECK_HRESULT(m_pImport->GetClassLayout( db.m_sig.m_sigField.m_token, NULL, NULL, 0, NULL, &ulSize ));
                    }
                    else
                    {
                        ulSize = SizeFromElementType( et );
                    }
                                                                
                    pValue = VA;                                            
                    len    = ulSize;
                }
            }
        }
                
        if(pValue)
        {                    
            _ASSERTE(len > 0);
            db.SetValue( pValue, len );
        }

        m_mapDef_Field.insert( FieldDefMap::value_type( fd, db ) );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::Parser::GetTypeMethod( mdMethodDef md )
{
    TINYCLR_HEADER();
    
    MethodDef db( this );

    if(m_mapDef_Method.find( md ) == m_mapDef_Method.end())
    {        
        PCCOR_SIGNATURE pSigBlob;
        ULONG           cbSigBlob;

        HELPER_QUICKSTRING_DECL(buf,size);

        while(true)
        {
            TINYCLR_CHECK_HRESULT(m_pImport->GetMethodProps( md, &db.m_td, HELPER_QUICKSTRING_CALL(buf,size), &db.m_flags, &pSigBlob, &cbSigBlob, &db.m_RVA, &db.m_implFlags ));

            HELPER_QUICKSTRING_CHECK(buf,size);
        }

        db.m_md   = md;
        db.m_name = buf.Ptr();        

        TINYCLR_CHECK_HRESULT(db.m_method.Parse( pSigBlob ));

        if(m_fNoByteCode == false)
        {
            TINYCLR_CHECK_HRESULT(ParseByteCode( db ));
        }

        TINYCLR_CHECK_HRESULT(EnumGenericParams( md ));

        m_mapDef_Method.insert( MethodDefMap::value_type( md, db ) );
    }

    TINYCLR_CLEANUP();
        
    if(FAILED(hr))
    {
        std::wstring str;

        if(SUCCEEDED(ErrorReporting::ConstructErrorOrigin( str, m_pSymReader, md, 0 )))
        {
            ErrorReporting::Print( str.c_str(), NULL, TRUE, 0, L"Cannot parse method signature '%s'", db.m_name.c_str() );
        }
    }

    TINYCLR_CLEANUP_END();
}

HRESULT MetaData::Parser::GetTypeInterface( mdInterfaceImpl ii )
{
    TINYCLR_HEADER();

    if(m_mapDef_Interface.find( ii ) == m_mapDef_Interface.end())
    {
        InterfaceImpl& db = m_mapDef_Interface[ii];

        TINYCLR_CHECK_HRESULT(m_pImport->GetInterfaceImplProps( ii, &db.m_td, &db.m_itf ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::Parser::GetCustomAttribute( mdCustomAttribute ca )
{
    TINYCLR_HEADER();

    if(m_mapDef_CustomAttribute.find( ca ) == m_mapDef_CustomAttribute.end())
    {
        CustomAttribute db( this );
        void const*     pBlob;
        ULONG           cbSize;

        TINYCLR_CHECK_HRESULT(m_pImport->GetCustomAttributeProps( ca, &db.m_tkObj, &db.m_tkType, &pBlob, &cbSize ));

        switch(TypeFromToken( db.m_tkObj ))
        {
        case mdtTypeDef  :
        case mdtFieldDef :
        case mdtMethodDef:
            switch(TypeFromToken( db.m_tkType ))
            {
            case mdtMemberRef:
            case mdtMethodDef:
                db.m_blob.resize( cbSize ); memcpy( &db.m_blob[0], pBlob, cbSize );

                m_mapDef_CustomAttribute.insert( CustomAttributeMap::value_type( ca, db ) );
                break;
            }
            break;
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::Parser::GetTypeSpec( mdTypeSpec ts )
{
    TINYCLR_HEADER();

    if(m_mapSpec_Type.find( ts ) == m_mapSpec_Type.end())
    {
        TypeSpec        db( this );
        PCCOR_SIGNATURE pSigBlob;
        ULONG           cbSigBlob;

        TINYCLR_CHECK_HRESULT(m_pImport->GetTypeSpecFromToken( ts, &pSigBlob, &cbSigBlob ));

        db.m_ts = ts;

        TINYCLR_CHECK_HRESULT(db.m_sig.Parse( pSigBlob ));

        m_mapSpec_Type.insert( TypeSpecMap::value_type( ts, db ) );
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::Parser::GetUserString( mdString s )
{
    TINYCLR_HEADER();

    if(m_mapDef_String.find( s ) == m_mapDef_String.end())
    {
        HELPER_QUICKSTRING_DECL(buf,size);

        while(true)
        {
            TINYCLR_CHECK_HRESULT(m_pImport->GetUserString( s, HELPER_QUICKSTRING_CALL(buf,size) ));

            HELPER_QUICKSTRING_CHECK(buf,size);
        }

        m_mapDef_String[s] = buf.Ptr();
    }

    TINYCLR_NOCLEANUP();
}
//--//

HRESULT MetaData::Parser::GetManifestResource( mdManifestResource mr )
{
    TINYCLR_HEADER();

    if(m_mapDef_ManifestResource.find( mr ) == m_mapDef_ManifestResource.end())
    {
        ManifestResource db( this );
        mdToken          tkImplementation;
        DWORD            dwOffset;
        DWORD            dwResourceFlags;
        BYTE*            pResource;
        DWORD            dwSize;

        HELPER_QUICKSTRING_DECL(buf,size);

        while(true)
        {
            TINYCLR_CHECK_HRESULT(m_pAssemblyImport->GetManifestResourceProps( mr, HELPER_QUICKSTRING_CALL(buf,size), &tkImplementation, &dwOffset, &dwResourceFlags ));

            HELPER_QUICKSTRING_CHECK(buf,size);
        }

        if(m_pe.GetResource( dwOffset, pResource, dwSize ) == false)
        {
            wprintf( L"Failed to get data for ManifestResource '%s'\n", buf.Ptr() );
            TINYCLR_SET_AND_LEAVE(CLR_E_ENTRY_NOT_FOUND);
        }

        db.m_mr               = mr;
        db.m_name             = buf.Ptr();
        db.m_tkImplementation = tkImplementation;
        db.m_dwResourceFlags  = dwResourceFlags;

        db.m_blob.resize( dwSize ); memcpy( &db.m_blob[0], pResource, dwSize );

        m_mapDef_ManifestResource.insert( ManifestResourceMap::value_type( mr, db ) );
    }

    TINYCLR_NOCLEANUP();
}


//--//

HRESULT MetaData::Parser::EnumAssemblyRefs()
{
    TINYCLR_HEADER();

    HCORENUM      num = NULL;
    mdAssemblyRef data[4];
    ULONG         count;

    while(true)
    {
        TINYCLR_CHECK_HRESULT(m_pAssemblyImport->EnumAssemblyRefs( &num, data, ARRAYSIZE(data), &count ));
        if(hr == S_FALSE || count == 0) break;

        for(ULONG i=0; i<count; i++)
        {
            mdAssemblyRef ar = data[i];

            TINYCLR_CHECK_HRESULT(GetAssemblyRef( ar ));
        }
    }


    TINYCLR_CLEANUP();

    if(num) m_pAssemblyImport->CloseEnum( num );

    TINYCLR_CLEANUP_END();
}

HRESULT MetaData::Parser::EnumModuleRefs()
{
    TINYCLR_HEADER();

    HCORENUM    num = NULL;
    mdModuleRef data[4];
    ULONG       count;

    while(true)
    {
        TINYCLR_CHECK_HRESULT(m_pImport->EnumModuleRefs( &num, data, ARRAYSIZE(data), &count ));
        if(hr == S_FALSE || count == 0) break;

        for(ULONG i=0; i<count; i++)
        {
            mdModuleRef mr = data[i];

            TINYCLR_CHECK_HRESULT(GetModuleRef( mr ));
        }
    }


    TINYCLR_CLEANUP();

    if(num) m_pAssemblyImport->CloseEnum( num );

    TINYCLR_CLEANUP_END();
}

HRESULT MetaData::Parser::EnumTypeRefs()
{
    TINYCLR_HEADER();

    HCORENUM  num = NULL;
    mdTypeRef data[4];
    ULONG     count;

    while(true)
    {
        TINYCLR_CHECK_HRESULT(m_pImport->EnumTypeRefs( &num, data, ARRAYSIZE(data), &count ));
        if(hr == S_FALSE || count == 0) break;

        for(ULONG i=0; i<count; i++)
        {
            mdTypeRef tr = data[i];

            TINYCLR_CHECK_HRESULT(GetTypeRef( tr ));
        }
    }


    TINYCLR_CLEANUP();

    if(num) m_pImport->CloseEnum( num );

    TINYCLR_CLEANUP_END();
}

HRESULT MetaData::Parser::EnumMemberRefs( TypeRef& tr )
{
    TINYCLR_HEADER();

    HCORENUM    num = NULL;
    mdMemberRef data[4];
    ULONG       count;

    while(true)
    {
        TINYCLR_CHECK_HRESULT(m_pImport->EnumMemberRefs( &num, tr.m_tr, data, ARRAYSIZE(data), &count ));
        if(hr == S_FALSE || count == 0) break;

        for(ULONG i=0; i<count; i++)
        {
            mdMemberRef mr = data[i];

            TINYCLR_CHECK_HRESULT(GetMemberRef( mr ));

            tr.m_lst.push_back( mr );
        }
    }


    TINYCLR_CLEANUP();

    if(num) m_pImport->CloseEnum( num );

    TINYCLR_CLEANUP_END();
}

HRESULT MetaData::Parser::EnumTypeDefs()
{
    TINYCLR_HEADER();

    HCORENUM  num = NULL;
    // Buffer for 100 type tokens ( 400 bytes ). One call to EnumTypeDefs reads 100 records.
    mdTypeDef data[100];  
    ULONG     count;

    while(true)
    {
        TINYCLR_CHECK_HRESULT(m_pImport->EnumTypeDefs( &num, data, ARRAYSIZE(data), &count ));
        if(hr == S_FALSE || count == 0) break;

        for(ULONG i=0; i<count; i++)
        {
            mdTypeDef td = data[i];

            TINYCLR_CHECK_HRESULT(GetTypeDef( td ));
        }
    }

    for(TypeDefMapIter itTypeDef = m_mapDef_Type.begin(); itTypeDef != m_mapDef_Type.end(); itTypeDef++)
    {
        TypeDef& td  = itTypeDef->second;

        TINYCLR_CHECK_HRESULT(EnumTypeFields    ( td      ));
        TINYCLR_CHECK_HRESULT(EnumTypeMethods   ( td      ));
        TINYCLR_CHECK_HRESULT(EnumTypeInterfaces( td      ));
        TINYCLR_CHECK_HRESULT(EnumGenericParams ( td.m_td ));
    }

    TINYCLR_CLEANUP();

    if(num) m_pImport->CloseEnum( num );

    TINYCLR_CLEANUP_END();
}

HRESULT MetaData::Parser::EnumTypeFields( TypeDef& td )
{
    TINYCLR_HEADER();

    HCORENUM   num = NULL;
    mdFieldDef data[4];
    ULONG      count;

    while(true)
    {
        TINYCLR_CHECK_HRESULT(m_pImport->EnumFields( &num, td.m_td, data, ARRAYSIZE(data), &count ));
        if(hr == S_FALSE || count == 0) break;

        for(ULONG i=0; i<count; i++)
        {
            mdFieldDef fd = data[i];

            TINYCLR_CHECK_HRESULT(GetTypeField( fd ));

            td.m_fields.push_back( fd );
        }
    }


    TINYCLR_CLEANUP();

    if(num) m_pImport->CloseEnum( num );

    TINYCLR_CLEANUP_END();
}

HRESULT MetaData::Parser::EnumTypeMethods( TypeDef& td )
{
    TINYCLR_HEADER();

    HCORENUM    num = NULL;
    mdMethodDef data[4];
    ULONG       count;

    while(true)
    {
        TINYCLR_CHECK_HRESULT(m_pImport->EnumMethods( &num, td.m_td, data, ARRAYSIZE(data), &count ));
        if(hr == S_FALSE || count == 0) break;

        for(ULONG i=0; i<count; i++)
        {
            mdMethodDef md = data[i];

            TINYCLR_CHECK_HRESULT(GetTypeMethod( md ));

            td.m_methods.push_back( md );
        }
    }


    TINYCLR_CLEANUP();

    if(num) m_pImport->CloseEnum( num );

    TINYCLR_CLEANUP_END();
}

HRESULT MetaData::Parser::EnumTypeInterfaces( TypeDef& td )
{
    TINYCLR_HEADER();

    HCORENUM        num = NULL;
    mdInterfaceImpl data[4];
    ULONG           count;

    while(true)
    {
        TINYCLR_CHECK_HRESULT(m_pImport->EnumInterfaceImpls( &num, td.m_td, data, ARRAYSIZE(data), &count ));
        if(hr == S_FALSE || count == 0) break;

        for(ULONG i=0; i<count; i++)
        {
            mdInterfaceImpl ii = data[i];

            TINYCLR_CHECK_HRESULT(GetTypeInterface( ii ));

            td.m_interfaces.push_back( ii );
        }
    }


    TINYCLR_CLEANUP();

    if(num) m_pImport->CloseEnum( num );

    TINYCLR_CLEANUP_END();
}

HRESULT MetaData::Parser::EnumCustomAttributes()
{
    TINYCLR_HEADER();

    HCORENUM          num = NULL;
    mdCustomAttribute data[4];
    ULONG             count;

    while(true)
    {
        TINYCLR_CHECK_HRESULT(m_pImport->EnumCustomAttributes( &num, 0, 0, data, ARRAYSIZE(data), &count ));
        if(hr == S_FALSE || count == 0) break;

        for(ULONG i=0; i<count; i++)
        {
            mdCustomAttribute ca = data[i];

            TINYCLR_CHECK_HRESULT(GetCustomAttribute( ca ));
        }
    }


    TINYCLR_CLEANUP();

    if(num) m_pImport->CloseEnum( num );

    TINYCLR_CLEANUP_END();
}

HRESULT MetaData::Parser::EnumTypeSpecs()
{
    TINYCLR_HEADER();

    HCORENUM   num = NULL;
    mdTypeSpec data[4];
    ULONG      count;

    while(true)
    {
        TINYCLR_CHECK_HRESULT(m_pImport->EnumTypeSpecs( &num, data, ARRAYSIZE(data), &count ));
        if(hr == S_FALSE || count == 0) break;

        for(ULONG i=0; i<count; i++)
        {
            mdTypeSpec ts = data[i];

            TINYCLR_CHECK_HRESULT(GetTypeSpec( ts ));
        }
    }


    TINYCLR_CLEANUP();

    if(num) m_pImport->CloseEnum( num );

    TINYCLR_CLEANUP_END();
}

HRESULT MetaData::Parser::EnumUserStrings()
{
    TINYCLR_HEADER();

    HCORENUM num = NULL;
    mdString data[4];
    ULONG    count;

    while(true)
    {
        TINYCLR_CHECK_HRESULT(m_pImport->EnumUserStrings( &num, data, ARRAYSIZE(data), &count ));
        if(hr == S_FALSE || count == 0) break;

        for(ULONG i=0; i<count; i++)
        {
            mdString s  = data[i];

            TINYCLR_CHECK_HRESULT(GetUserString( s ));
        }
    }


    TINYCLR_CLEANUP();

    if(num) m_pImport->CloseEnum( num );

    TINYCLR_CLEANUP_END();
}

HRESULT MetaData::Parser::EnumManifestResources()
{
    TINYCLR_HEADER();

    HCORENUM           num = NULL;
    mdManifestResource data[4];
    ULONG              count;

    while(true)
    {
        TINYCLR_CHECK_HRESULT(m_pAssemblyImport->EnumManifestResources( &num, data, ARRAYSIZE(data), &count ));
        if(hr == S_FALSE || count == 0) break;

        for(ULONG i=0; i<count; i++)
        {
            mdManifestResource mr = data[i];

            TINYCLR_CHECK_HRESULT(GetManifestResource( mr ));
        }
    }


    TINYCLR_CLEANUP();

    if(num) m_pImport->CloseEnum( num );

    TINYCLR_CLEANUP_END();
}

HRESULT MetaData::Parser::EnumGenericParams( mdToken tk )
{
    TINYCLR_HEADER();

    HCORENUM       num = NULL;
    mdGenericParam data[4];
    ULONG          count;

    if(!m_pImport2) TINYCLR_SET_AND_LEAVE(S_OK);

    while(true)
    {
        TINYCLR_CHECK_HRESULT(m_pImport2->EnumGenericParams( &num, tk, data, ARRAYSIZE(data), &count ));
        if(hr == S_FALSE || count == 0) break;

        TINYCLR_SET_AND_LEAVE(CLR_E_PARSER_UNSUPPORTED_GENERICS);
    }


    TINYCLR_CLEANUP();    

    if(num) m_pImport2->CloseEnum( num );

    TINYCLR_CLEANUP_END();
}

//--//

static bool StringEndsWithSuffix( LPCWSTR str1, LPCWSTR str2 )
{
    size_t len1 = wcslen( str1 );
    size_t len2 = wcslen( str2 );

    return (len1 >= len2 && _wcsicmp( &str1[len1-len2], str2 ) == 0);
}

const CLSID CLSID_CorSymBinder_SxS = {0x0A29FF9E,0x7F9C,0x4437,{0x8B,0x11,0xF4,0x24,0x49,0x1E,0x39,0x31}};
const IID IID_ISymUnmanagedBinder = {0xAA544D42,0x28CB,0x11d3,{0xBD,0x22,0x00,0x00,0xF8,0x08,0x49,0xBD}};

HRESULT MetaData::Parser::ParseByteCode( MethodDef& db )
{
    TINYCLR_HEADER();

    if(db.m_RVA)
    {
        void* VA;

        if(m_pe.GetVAforRVA( db.m_RVA, VA ))
        {
            db.m_VA = (const BYTE*)VA;
        }
    }


    if(db.m_VA)
    {
        COR_ILMETHOD_DECODER il( (const COR_ILMETHOD*)db.m_VA );

        mdSignature ls = (mdSignature)il.GetLocalVarSigTok();
        if(ls)
        {
            PCCOR_SIGNATURE pSigBlob;
            ULONG           cbSigBlob;

            TINYCLR_CHECK_HRESULT(m_pImport->GetSigFromToken( ls, &pSigBlob, &cbSigBlob ));

            if(FAILED(hr = db.m_vars.Parse( pSigBlob )))
            {
                std::wstring str;

                if(SUCCEEDED(ErrorReporting::ConstructErrorOrigin( str, m_pSymReader, db.m_md, 0 )))
                {
                    ErrorReporting::Print( str.c_str(), NULL, TRUE, 0, L"Cannot parse locals signature for '%s'", db.m_name.c_str() );
                }

                TINYCLR_LEAVE();
            }
        }

        if(il.Code)
        {
            const CLR_UINT8* IP    = il.Code;
            const CLR_UINT8* IPend = IP + il.GetCodeSize();

            TINYCLR_CHECK_HRESULT(db.m_byteCode.Parse( m_mapDef_Type.find(db.m_td)->second, db, il ));
            db.m_byteCodeOriginal = db.m_byteCode;
            db.m_maxStack         = il.GetMaxStack();

            for(size_t i=0; i<db.m_byteCode.m_exceptions.size(); i++)
            {
                ByteCode::LogicalExceptionBlock& ref = db.m_byteCode.m_exceptions[i];
                Parser*                          prDst;
                TypeDef*                         tdDst;

                //If not a filter or finally clause, lookup EH class token for filter clause.
                if((ref.m_Flags != COR_ILEXCEPTION_CLAUSE_FINALLY) && (ref.m_Flags != COR_ILEXCEPTION_CLAUSE_FILTER))
                {
                    if(!IsNilToken(ref.m_ClassToken))
                        TINYCLR_CHECK_HRESULT(m_holder->ResolveTypeDef( this, ref.m_ClassToken, prDst, tdDst ));
                }
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT MetaData::Parser::Analyze( LPCWSTR szFileName )
{
    ISymUnmanagedBinderPtr pBinder = NULL;

    TINYCLR_HEADER();

    m_holder->m_mapAssemblies[szFileName] = this;

    m_assemblyFile = szFileName;

    if(::IsDebuggerPresent())
    {
        wprintf( L"Analyzing %s...\n", szFileName );
    }

    m_pe.OpenAndDecode( szFileName );

    TINYCLR_CHECK_HRESULT(::CoCreateInstance( CLSID_CorMetaDataDispenser, NULL, CLSCTX_INPROC_SERVER, IID_IMetaDataDispenserEx, (void **)&m_pDisp ));

    TINYCLR_CHECK_HRESULT(m_pDisp->OpenScope( szFileName, ofRead, IID_IMetaDataImport, (IUnknown**)&m_pImport ));
    
    TINYCLR_CHECK_HRESULT(m_pImport->QueryInterface( IID_IMetaDataAssemblyImport, (void**)&m_pAssemblyImport ));

    //Optional interface
    m_pImport->QueryInterface( IID_IMetaDataImport2, (void**)&m_pImport2 );

    if(SUCCEEDED(CoCreateInstance( CLSID_CorSymBinder_SxS, NULL, CLSCTX_INPROC_SERVER, IID_ISymUnmanagedBinder, (void**)(&pBinder) )))
    {
        pBinder->GetReaderForFile( m_pAssemblyImport, this->m_assemblyFile.c_str(), NULL, &m_pSymReader );
    }

    {
        IMAGE_COR20_HEADER* pCorHeader;

        if(m_pe.GetCOMHeader( pCorHeader ) == false)
        {
            wprintf( L"Cannot find COR header...\n" );
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        m_entryPointToken = pCorHeader->EntryPointToken;
    }

    TINYCLR_CHECK_HRESULT(GetAssemblyDef  ());
    /****************************************/
    TINYCLR_CHECK_HRESULT(EnumAssemblyRefs());
    TINYCLR_CHECK_HRESULT(EnumModuleRefs  ());
    TINYCLR_CHECK_HRESULT(EnumTypeRefs    ());
    /****************************************/
    TINYCLR_CHECK_HRESULT(EnumTypeDefs    ());
    TINYCLR_CHECK_HRESULT(EnumTypeSpecs   ());
    TINYCLR_CHECK_HRESULT(EnumUserStrings ());

    if(m_fNoAttributes == false)
    {
        //Manifest resources are currently ignored.
        //-ImportResource to grab .tinyresources format.
        //TINYCLR_CHECK_HRESULT(EnumManifestResources());

        TINYCLR_CHECK_HRESULT(EnumCustomAttributes());

        for(CustomAttributeMapIter itCA = m_mapDef_CustomAttribute.begin(); itCA != m_mapDef_CustomAttribute.end(); )
        {
            CustomAttributeMapIter itCA2  = itCA++;
            CustomAttribute&       ca     = itCA2->second;
            bool                   fErase = false;

            TINYCLR_CHECK_HRESULT(ca.Parse());

            if(ca.m_nameOfAttributeClass == L"Microsoft.SPOT.NativeProfilerAttribute")
            {
                m_setAttributes_Methods_NativeProfiler.insert( ca.m_tkObj );
                fErase = true;
            }

            if(ca.m_nameOfAttributeClass == L"Microsoft.SPOT.GloballySynchronizedAttribute")
            {
                m_setAttributes_Methods_GloballySynchronized.insert( ca.m_tkObj );
                fErase = true;
            }

            if(ca.m_nameOfAttributeClass == L"Microsoft.SPOT.FieldNoReflectionAttribute" ||
               ca.m_nameOfAttributeClass == L"System.Reflection.FieldNoReflectionAttribute")
            {
                m_setAttributes_Fields_NoReflection.insert( ca.m_tkObj );
                fErase = true;
            }

            if(ca.m_nameOfAttributeClass == L"Microsoft.SPOT.PublishInApplicationDirectoryAttribute")
            {
                m_setAttributes_Types_PublishInApplicationDirectory.insert( ca.m_tkObj );
                fErase = true;
            }

            if(m_setFilter_ExcludeClassByName.find( ca.m_nameOfAttributeClass ) != m_setFilter_ExcludeClassByName.end())
            {
                fErase = true;
            }

            if(fErase)
            {
                m_mapDef_CustomAttribute.erase( itCA2 );
            }
        }
    }

    if(m_fNoByteCode == false)
    {
        for(MethodDefMapIter itMD = m_mapDef_Method.begin(); itMD != m_mapDef_Method.end(); itMD++)
        {
            MethodDef& md = itMD->second;

            if(md.m_byteCode.m_opcodes.size() > 0)
            {
                TINYCLR_CHECK_HRESULT(md.m_byteCode.VerifyConsistency( m_mapDef_Type.find(md.m_td)->second, md, this ));
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT MetaData::Parser::CanIncludeMember( mdToken tk, mdToken tm )
{
    TINYCLR_HEADER();

    if(m_setFilter_ExcludeClassByName.size() > 0)
    {
        std::wstring str; TokenToString( tk, str );

        if(m_setFilter_ExcludeClassByName.find( str ) != m_setFilter_ExcludeClassByName.end())
        {
            TINYCLR_SET_AND_LEAVE(S_FALSE);
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::Parser::BuildDependencyList( mdToken tk, mdTokenSet& set )
{
    TINYCLR_HEADER();

    switch(TypeFromToken( tk ))
    {
    case mdtTypeRef:
        {
            TINYCLR_CHECK_HRESULT(GetTypeRef( tk ));

            TypeRef& tr = m_mapRef_Type[tk];

            switch(TypeFromToken( tr.m_scope ))
            {
            case mdtAssemblyRef:
            case mdtTypeRef:
                SetReference( set, tr.m_scope );
                break;
            }
        }
        break;

    case mdtMemberRef:
        {
            TINYCLR_CHECK_HRESULT(GetMemberRef( tk ));

            MemberRef& mr = m_mapRef_Member.find( tk )->second;

            SetReference( set, mr.m_tr );

            mr.m_sig.ExtractTypeRef( set );
        }
        break;

    case mdtTypeSpec:
        {
            TINYCLR_CHECK_HRESULT(GetTypeSpec( tk ));

            TypeSpec& ts = m_mapSpec_Type.find( tk )->second;

            ts.m_sig.ExtractTypeRef( set );
        }
        break;

    case mdtTypeDef:
        {
            TINYCLR_CHECK_HRESULT(GetTypeDef( tk ));

            TypeDef& td = m_mapDef_Type.find(tk)->second;

            SetReference( set, td.m_extends        );
            SetReference( set, td.m_enclosingClass );

            TINYCLR_CHECK_HRESULT(IncludeAttributes( tk, set ));

            for(mdFieldDefListIter itFieldDef = td.m_fields.begin(); itFieldDef != td.m_fields.end(); itFieldDef++)
            {
                mdToken   tf = (mdToken)*itFieldDef;
                FieldDef& fd = m_mapDef_Field.find( tf )->second;

                if(fd.m_flags & fdLiteral) continue; // Don't generate constant fields.

                TINYCLR_CHECK_HRESULT(CanIncludeMember( tk, tf )); if(hr == S_FALSE) continue;

                SetReference( set, tf );
            }

            for(mdMethodDefListIter itMethodDef = td.m_methods.begin(); itMethodDef != td.m_methods.end(); itMethodDef++)
            {
                mdToken    tm = (mdToken)*itMethodDef;
                MethodDef& md = m_mapDef_Method.find( tm )->second;

                TINYCLR_CHECK_HRESULT(CanIncludeMember( tk, tm )); if(hr == S_FALSE) continue;

                SetReference( set, tm );
            }

            for(mdInterfaceImplListIter itInterfaceImpl = td.m_interfaces.begin(); itInterfaceImpl != td.m_interfaces.end(); itInterfaceImpl++)
            {
                mdToken        ti = (mdToken)*itInterfaceImpl;
                InterfaceImpl& ii = m_mapDef_Interface[ti];

                TINYCLR_CHECK_HRESULT(CanIncludeMember( tk, ti )); if(hr == S_FALSE) continue;

                SetReference( set, ti );
            }
        }
        break;

    case mdtFieldDef:
        {
            TINYCLR_CHECK_HRESULT(GetTypeField( tk ));

            FieldDef& fd = m_mapDef_Field.find( tk )->second;

            fd.m_sig.ExtractTypeRef( set );

            TINYCLR_CHECK_HRESULT(IncludeAttributes( tk, set ));
        }
        break;

    case mdtMethodDef:
        {
            TINYCLR_CHECK_HRESULT(GetTypeMethod( tk ));

            MethodDef& md = m_mapDef_Method.find( tk )->second;

            md.m_method.ExtractTypeRef( set );

            if(m_fNoByteCode == false)
            {
                md.m_vars.ExtractTypeRef( set );

                for(size_t i=0; i<md.m_byteCode.m_opcodes.size(); i++)
                {
                    ByteCode::LogicalOpcodeDesc& ref = md.m_byteCode.m_opcodes[i];

                    if(ref.m_token != mdTokenNil)
                    {
                        SetReference( set, ref.m_token );
                    }
                }

                for(size_t i=0; i<md.m_byteCode.m_exceptions.size(); i++)
                {
                    ByteCode::LogicalExceptionBlock& ref = md.m_byteCode.m_exceptions[i];
                    
                    if (ref.m_Flags != COR_ILEXCEPTION_CLAUSE_FILTER)
                        SetReference( set, ref.m_ClassToken );
                }
            }

            TINYCLR_CHECK_HRESULT(IncludeAttributes( tk, set ));
        }
        break;

    case mdtInterfaceImpl:
        {
            TINYCLR_CHECK_HRESULT(GetTypeInterface( tk ));

            InterfaceImpl& ii = m_mapDef_Interface[tk];

            SetReference( set, ii.m_itf );
            SetReference( set, ii.m_td  );
        }
        break;
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::Parser::IncludeAttributes( mdToken tk, mdTokenSet& set )
{
    TINYCLR_HEADER();
    
    WCHAR* ignoreAttributes[] = {
        // Don't include any compile-time attributes.
        L"System.AttributeUsageAttribute",
        L"System.CLSCompliantAttribute",
        L"System.FlagsAttribute",
        L"System.ObsoleteAttribute",
        L"System.Diagnostics.ConditionalAttribute",
        // Don't include any debugging attributes
        L"System.Diagnostics.DebuggableAttribute",
        L"System.Diagnostics.DebuggerHiddenAttribute",
        L"System.Diagnostics.DebuggerNonUserCodeAttribute",
        L"System.Diagnostics.DebuggerStepThroughAttribute",
        L"System.Diagnostics.DebuggerDisplayAttribute",
        //Don't include any Intellisense filtering attributes
        L"System.ComponentModel.EditorBrowsableAttribute",
        //Not supported
        L"System.Reflection.DefaultMemberAttribute",
        //For AssemblyInfo.cs template and designers
        L"System.Reflection.AssemblyFileVersionAttribute",
        L"System.Runtime.InteropServices.ComVisibleAttribute",
        L"System.Runtime.InteropServices.GuidAttribute",
        // Don't include any VB-generated attributes
        L"Microsoft.VisualBasic.ComClassAttribute",
        L"Microsoft.VisualBasic.HideModuleNameAttribute",
        L"Microsoft.VisualBasic.MyGroupCollectionAttribute",
        L"Microsoft.VisualBasic.VBFixedArrayAttribute",
        L"Microsoft.VisualBasic.VBFixedStringAttribute",
        L"Microsoft.VisualBasic.CompilerServices.DesignerGeneratedAttribute",
        L"Microsoft.VisualBasic.CompilerServices.OptionCompareAttribute",
        L"Microsoft.VisualBasic.CompilerServices.OptionTextAttribute",
        L"Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute",
    };

    for(CustomAttributeMapIter itCA = m_mapDef_CustomAttribute.begin(); itCA != m_mapDef_CustomAttribute.end(); itCA++)
    {
        CustomAttribute& ca = itCA->second;

        if(ca.m_tkObj == tk)
        {
            Parser*    prDst;
            MethodDef* mdDst;
            TypeDef*   tdDst;
            bool       fIgnore;

            TINYCLR_CHECK_HRESULT(m_holder->ResolveMethodDef( this , ca.m_tkType, prDst, mdDst ));
            TINYCLR_CHECK_HRESULT(m_holder->ResolveTypeDef  ( prDst, mdDst->m_td, prDst, tdDst ));

            fIgnore = false;
            for(int i = 0; i < (int)(ARRAYSIZE(ignoreAttributes)); i++)
            {
                if(wcscmp(tdDst->m_name.c_str(), ignoreAttributes[i]) == 0)
                {                    
                    fIgnore = true;
                    break;
                }
            }

            if(!fIgnore)
            {
                set.insert( ca.m_tkType );
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::Parser::RemoveUnused()
{
    TINYCLR_HEADER();

    mdTokenSet set;
    mdTokenSet setNew;

    for(TypeDefMapIter itTypeDef = m_mapDef_Type.begin(); itTypeDef != m_mapDef_Type.end(); itTypeDef++)
    {
        mdToken  tk  = itTypeDef->second.m_td;
        TypeDef& td  = itTypeDef->second;

        std::wstring str; TokenToString( tk, str );

        if(m_setFilter_ExcludeClassByName.find( str ) != m_setFilter_ExcludeClassByName.end()) continue;

        setNew.insert( tk );
    }


    while(setNew.size())
    {
        mdTokenSet     setAdd;
        mdTokenSetIter it;

        for(it = setNew.begin(); it != setNew.end(); it++)
        {
            mdTokenSet setTmp;
            mdToken    tk = (mdToken)*it;

            set.insert( tk );

            if(m_fVerboseMinimize)
            {
                std::wstring to; TokenToString( tk, to );

                wprintf( L"Including %s\n", to.c_str() );
            }


            TINYCLR_CHECK_HRESULT(BuildDependencyList( tk, setTmp ));

            if(m_fVerboseMinimize)
            {
                Dump_ShowDependencies( tk, set, setTmp );
            }

            for(mdTokenSetIter it2 = setTmp.begin(); it2 != setTmp.end(); it2++)
            {
                setAdd.insert( *it2 );
            }
        }

        setNew.clear();

        for(it = setAdd.begin(); it != setAdd.end(); it++)
        {
            if(set.find( *it ) == set.end())
            {
                setNew.insert( *it );
            }
        }
    }

    //--//

    RemoveUnusedItems( m_mapRef_Assembly , set );
    RemoveUnusedItems( m_mapRef_Module   , set );
    RemoveUnusedItems( m_mapRef_Type     , set );
    RemoveUnusedItems( m_mapRef_Member   , set );

    RemoveUnusedItems( m_mapDef_Type     , set );
    RemoveUnusedItems( m_mapDef_Field    , set );
    RemoveUnusedItems( m_mapDef_Method   , set );
    RemoveUnusedItems( m_mapDef_Interface, set );

    RemoveUnusedItems( m_mapSpec_Type    , set );

    RemoveUnusedItems( m_mapDef_String   , set );

    //--//

    //
    // Remove unused attributes.
    //
    for(CustomAttributeMapIter itCA = m_mapDef_CustomAttribute.begin(); itCA != m_mapDef_CustomAttribute.end(); )
    {
        CustomAttributeMapIter itCA2 = itCA++;
        CustomAttribute&       ca    = itCA2->second;

        if(CheckIsTokenPresent( ca.m_tkObj ) && CheckIsTokenPresent( ca.m_tkType ))
        {
            continue;
        }

        m_mapDef_CustomAttribute.erase( itCA2 );
    }

    //
    // Renormalize type definition look-up lists.
    //
    for(TypeDefMapIter itTypeDef = m_mapDef_Type.begin(); itTypeDef != m_mapDef_Type.end(); itTypeDef++)
    {
        TypeDef& td = itTypeDef->second;

        for(mdFieldDefListIter itFieldDef = td.m_fields.begin(); itFieldDef != td.m_fields.end(); )
        {
            if(IsTokenPresent( m_mapDef_Field, (mdToken)*itFieldDef ) == false)
            {
                td.m_fields.erase( itFieldDef++ );
            }
            else
            {
                itFieldDef++;
            }
        }

        for(mdMethodDefListIter itMethodDef = td.m_methods.begin(); itMethodDef != td.m_methods.end(); )
        {
            if(IsTokenPresent( m_mapDef_Method, (mdToken)*itMethodDef ) == false)
            {
                td.m_methods.erase( itMethodDef++ );
            }
            else
            {
                itMethodDef++;
            }
        }

        for(mdMethodDefListIter itMethodDef = td.m_methods.begin(); itMethodDef != td.m_methods.end(); )
        {
            if(IsTokenPresent( m_mapDef_Method, (mdToken)*itMethodDef ) == false)
            {
                td.m_methods.erase( itMethodDef++ );
            }
            else
            {
                itMethodDef++;
            }
        }

        for(mdInterfaceImplListIter itInterfaceImpl = td.m_interfaces.begin(); itInterfaceImpl != td.m_interfaces.end(); )
        {
            if(IsTokenPresent( m_mapDef_Interface, (mdToken)*itInterfaceImpl ) == false)
            {
                td.m_interfaces.erase( itInterfaceImpl++ );
            }
            else
            {
                itInterfaceImpl++;
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT MetaData::Parser::VerifyConsistency()
{
    TINYCLR_HEADER();


    for(TypeSpecMapIter itTypeSpec = m_mapSpec_Type.begin(); itTypeSpec != m_mapSpec_Type.end(); itTypeSpec++)
    {
        TypeSpec&      ts  = itTypeSpec->second;
        TypeSignature* sig = &ts.m_sig;

        while(sig)
        {
            if(sig->m_opt == ELEMENT_TYPE_ARRAY)
            {
                ErrorReporting::Print( m_assemblyName.c_str(), NULL, TRUE, 0, L"Unsupported multi-dimensional array: 0x%08X ", itTypeSpec->first );

                TINYCLR_SET_AND_LEAVE(CLR_E_PARSER_UNSUPPORTED_MULTIDIM_ARRAY);
            }

            sig = sig->m_sub;
        }
    }

    for(MemberRefMapIter itMemberRef = m_mapRef_Member.begin(); itMemberRef != m_mapRef_Member.end(); itMemberRef++)
    {
        MetaData::MemberRef& mr = itMemberRef->second;

        switch(mr.m_sig.m_type)
        {
        case mdtFieldDef:
        case mdtMethodDef:
            break;

        default:
            ErrorReporting::Print( m_assemblyName.c_str(), NULL, TRUE, 0, L"Unsupported member reference: 0x%08X : %s (Class: 0x%08X) (Signature: 0x%08X)\n", itMemberRef->first, mr.m_name.c_str(), mr.m_tr, mr.m_sig.m_type );
            TINYCLR_SET_AND_LEAVE(CLR_E_PARSER_UNKNOWN_MEMBER_REF);
        }
    }

    for(TypeDefMapIter itTypeDef = m_mapDef_Type.begin(); itTypeDef != m_mapDef_Type.end(); itTypeDef++)
    {
        TypeDef& td = itTypeDef->second;

        TINYCLR_CHECK_HRESULT(CheckTokenPresence( td.m_extends        ));
        TINYCLR_CHECK_HRESULT(CheckTokenPresence( td.m_enclosingClass ));

        for(mdFieldDefListIter itFieldDef = td.m_fields.begin(); itFieldDef != td.m_fields.end(); itFieldDef++)
        {
            FieldDefMapIter it = m_mapDef_Field.find( *itFieldDef );
            mdTokenSet      set;

            if(it == m_mapDef_Field.end())
            {
                ErrorReporting::Print( m_assemblyName.c_str(), NULL, TRUE, 0, L"Not all fields for type '%s' can be resolved.\n", td.m_name.c_str() );
                TINYCLR_CHECK_HRESULT(CLR_E_PARSER_MISSING_FIELD);
            }
            else
            {
                FieldDef& fd = it->second;

                fd.m_sig.ExtractTypeRef( set ); TINYCLR_CHECK_HRESULT(CheckTokensPresence( set ));
            }
        }

        for(mdMethodDefListIter itMethodDef = td.m_methods.begin(); itMethodDef != td.m_methods.end(); itMethodDef++)
        {
            MethodDefMapIter it = m_mapDef_Method.find( *itMethodDef );
            mdTokenSet       set;

            if(it == m_mapDef_Method.end())
            {
                ErrorReporting::Print( m_assemblyName.c_str(), NULL, TRUE, 0, L"Not all methods for type '%s' can be resolved.\n", td.m_name.c_str() );
                TINYCLR_CHECK_HRESULT(CLR_E_PARSER_MISSING_METHOD);
            }
            else
            {
                MethodDef& md = it->second;

                md.m_method.ExtractTypeRef( set ); TINYCLR_CHECK_HRESULT(CheckTokensPresence( set ));

                if(m_fNoByteCode == false)
                {
                    md.m_vars.ExtractTypeRef( set ); TINYCLR_CHECK_HRESULT(CheckTokensPresence( set ));

                    for(size_t i=0; i<md.m_byteCode.m_opcodes.size(); i++)
                    {
                        ByteCode::LogicalOpcodeDesc& ref = md.m_byteCode.m_opcodes[i];

                        if(ref.m_token != mdTokenNil)
                        {
                            TINYCLR_CHECK_HRESULT(CheckTokenPresence( ref.m_token ));
                        }
                    }
                }
            }
        }

        for(mdInterfaceImplListIter itInterfaceImpl = td.m_interfaces.begin(); itInterfaceImpl != td.m_interfaces.end(); itInterfaceImpl++)
        {
            InterfaceImplMapIter it = m_mapDef_Interface.find( *itInterfaceImpl );

            if(it == m_mapDef_Interface.end())
            {
                ErrorReporting::Print( m_assemblyName.c_str(), NULL, TRUE, 0, L"Not all interfaces for type '%s' can be resolved.\n", td.m_name.c_str() );
                TINYCLR_CHECK_HRESULT(CLR_E_PARSER_MISSING_INTERFACE);
            }
            else
            {
                InterfaceImpl& ii = it->second;

                TINYCLR_CHECK_HRESULT(CheckTokenPresence( ii.m_itf ));
            }
        }
    }

    for(CustomAttributeMapIter itCA = m_mapDef_CustomAttribute.begin(); itCA != m_mapDef_CustomAttribute.end(); )
    {
        CustomAttributeMapIter itCA2 = itCA++;
        CustomAttribute&       ca    = itCA2->second;

        TINYCLR_CHECK_HRESULT(CheckTokenPresence( ca.m_tkObj  ));
        TINYCLR_CHECK_HRESULT(CheckTokenPresence( ca.m_tkType ));
    }

    TINYCLR_NOCLEANUP();
}

bool MetaData::Parser::CheckIsTokenPresent( mdToken tk )
{
    bool fPresent = IsNilToken(tk); // A null token is skipped.

    if(fPresent == false)
    {
        switch(TypeFromToken( tk ))
        {
        case mdtAssemblyRef  : fPresent = IsTokenPresent( m_mapRef_Assembly , tk ); break;
        case mdtModuleRef    : fPresent = IsTokenPresent( m_mapRef_Module   , tk ); break;
        case mdtTypeRef      : fPresent = IsTokenPresent( m_mapRef_Type     , tk ); break;
        case mdtMemberRef    : fPresent = IsTokenPresent( m_mapRef_Member   , tk ); break;

        case mdtTypeDef      : fPresent = IsTokenPresent( m_mapDef_Type     , tk ); break;
        case mdtFieldDef     : fPresent = IsTokenPresent( m_mapDef_Field    , tk ); break;
        case mdtMethodDef    : fPresent = IsTokenPresent( m_mapDef_Method   , tk ); break;
        case mdtInterfaceImpl: fPresent = IsTokenPresent( m_mapDef_Interface, tk ); break;

        case mdtTypeSpec     : fPresent = IsTokenPresent( m_mapSpec_Type    , tk ); break;

        case mdtString       : fPresent = IsTokenPresent( m_mapDef_String   , tk ); break;
        }
    }

    return fPresent;
}

HRESULT MetaData::Parser::CheckTokenPresence( mdToken tk )
{
    return CheckIsTokenPresent( tk ) ? S_OK : CLR_E_PARSER_MISSING_TOKEN;
}

HRESULT MetaData::Parser::CheckTokensPresence( mdTokenSet& set)
{
    TINYCLR_HEADER();

    for(mdTokenSetIter it = set.begin(); it != set.end(); it++)
    {
        TINYCLR_CHECK_HRESULT(CheckTokenPresence( (mdToken)*it ));
    }

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////

MetaData::Collection::Collection()
{
    // CLR_RT_StringSet m_setIgnoreAssemblies;
    // LoadHintsMap     m_mapLoadHints;
    // AssembliesMap    m_mapAssemblies;
}

MetaData::Collection::~Collection()
{
    Clear( true );
}

void MetaData::Collection::Clear( bool fAll )
{
    if(fAll)
    {
        m_setIgnoreAssemblies.clear();
        m_mapLoadHints       .clear();
    }

    for(AssembliesMapIter it = m_mapAssemblies.begin(); it != m_mapAssemblies.end(); it++)
    {
        delete it->second;
    }
    m_mapAssemblies.clear();
}

HRESULT MetaData::Collection::IgnoreAssembly( LPCWSTR szAssemblyName )
{
    TINYCLR_HEADER();

    m_setIgnoreAssemblies.insert( szAssemblyName );

    TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT MetaData::Collection::LoadHints( LPCWSTR szAssemblyName, LPCWSTR szFileName )
{
    TINYCLR_HEADER();

    m_mapLoadHints[szAssemblyName] = szFileName;

    TINYCLR_NOCLEANUP_NOLABEL();
}

bool MetaData::Collection::FileExists( const std::wstring& assemblyName, const std::wstring& targetPath, std::wstring& filename )
{
    std::wstring::size_type pos;

    pos = targetPath.find_last_of( '\\' );
    if(pos != std::wstring::npos)
    {
        filename = targetPath;
        filename.erase ( pos + 1 );
        filename.append( assemblyName );
        filename.append( L".DLL" );
    }
    else
    {
        filename = assemblyName;
        filename.append( L".DLL" );
    }

    return FileExists( filename );;
}

bool MetaData::Collection::FileExists( const std::wstring& filename )
{
    WIN32_FILE_ATTRIBUTE_DATA fad;

    if (::GetFileAttributesExW( filename.c_str(), GetFileExInfoStandard, &fad ))
    {
        return true;
    }
     
    return false;
}

HRESULT MetaData::Collection::FromNameToFile( const std::wstring& name, std::wstring& file )
{
    TINYCLR_HEADER();

    // First, check the LoadHints
    LoadHintsMapIter itLH = m_mapLoadHints.find( name );

    if(itLH != m_mapLoadHints.end())
    {
        file = itLH->second;
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    // Next, Try the directory of each of the loaded assembly
    for(AssembliesMapIter itASSM = m_mapAssemblies.begin(); itASSM != m_mapAssemblies.end(); itASSM++)
    {
        if(FileExists( name, itASSM->first, file ))
        {
            TINYCLR_SET_AND_LEAVE(S_OK);
        }
    }

    // Lastly, Try the directory of each of the hints given
    for(itLH = m_mapLoadHints.begin(); itLH != m_mapLoadHints.end(); itLH++)
    {
        if(FileExists( name, itLH->second, file ))
        {
            TINYCLR_SET_AND_LEAVE(S_OK);
        }
    }

    // If none of the above methods work, fail with an error
    ErrorReporting::Print( NULL, NULL, TRUE, 0, L"Cannot locate file for assembly '%s'\n", name.c_str() );
    TINYCLR_SET_AND_LEAVE(CLR_E_ENTRY_NOT_FOUND);

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::Collection::CreateDependentAssembly( LPCWSTR szFileName, Parser*& pr )
{
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT(CreateAssembly( pr ));

    pr->m_fNoByteCode   = true;
    pr->m_fNoAttributes = true;

    TINYCLR_CHECK_HRESULT(pr->Analyze( szFileName ));

    TINYCLR_NOCLEANUP();
}

HRESULT MetaData::Collection::CreateAssembly( Parser*& pr )
{
    TINYCLR_HEADER();

    pr = new Parser( this );

    TINYCLR_NOCLEANUP_NOLABEL();
}

//--//

bool MetaData::Collection::IsAssemblyToken( Parser* pr, mdToken tk )
{
    switch(TypeFromToken( tk ))
    {
    case mdtAssemblyRef:
        {
            AssemblyRefMapIter itAR = pr->m_mapRef_Assembly.find( tk );

            if(itAR != pr->m_mapRef_Assembly.end())
            {
                return true;
            }
        }
        break;
    }

    return false;
}

HRESULT MetaData::Collection::ResolveAssemblyDef( Parser* pr, mdToken tk, Parser*& prDst )
{
    TINYCLR_HEADER();

    prDst = NULL;

    switch(TypeFromToken( tk ))
    {
    case mdtAssemblyRef:
        {
            AssemblyRefMapIter itAR = pr->m_mapRef_Assembly.find( tk );

            if(itAR != pr->m_mapRef_Assembly.end())
            {
                AssemblyRef&  ar   = itAR->second;
                std::wstring& name = ar.m_name;
                std::wstring  file;

                if(m_setIgnoreAssemblies.find( name ) != m_setIgnoreAssemblies.end())
                {
                    ErrorReporting::Print( pr->m_assemblyFile.c_str(), NULL, TRUE, 0, L"Cannot resolve assembly reference, '%s' is marked as 'ignore'", name.c_str() );
                    TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
                }

                TINYCLR_CHECK_HRESULT(FromNameToFile( name, file ));

                AssembliesMapIter itAM = m_mapAssemblies.find( file );

                if(itAM != m_mapAssemblies.end())
                {
                    prDst = itAM->second;
                    TINYCLR_SET_AND_LEAVE(S_OK);
                }

                TINYCLR_CHECK_HRESULT(CreateDependentAssembly( file.c_str(), prDst ));

                TINYCLR_SET_AND_LEAVE(S_OK);
            }
        }
        break;
    }

    ErrorReporting::Print( pr->m_assemblyName.c_str(), NULL, TRUE, 0, L"Cannot resolve assembly %s::%08x!", pr->m_assemblyName.c_str(), tk );
    TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    TINYCLR_NOCLEANUP();
}

//--//

bool MetaData::Collection::IsTypeToken( Parser* pr, mdToken tk )
{
    switch(TypeFromToken( tk ))
    {
    case mdtTypeRef:
        {
            TypeRefMapIter itTR = pr->m_mapRef_Type.find( tk );

            if(itTR != pr->m_mapRef_Type.end())
            {
                return true;
            }
        }
        break;

    case mdtTypeDef:
        {
            TypeDefMapIter itTD = pr->m_mapDef_Type.find( tk );

            if(itTD != pr->m_mapDef_Type.end())
            {
                return true;
            }
        }
        break;
    }

    return false;
}

HRESULT MetaData::Collection::ResolveTypeDef( Parser* pr, mdToken tk, Parser*& prDst, TypeDef*& tdDst )
{
    TINYCLR_HEADER();

    std::wstring missing;

    prDst = NULL;
    tdDst = NULL;

    switch(TypeFromToken( tk ))
    {
    case mdtTypeRef:
        {
            TypeRefMapIter itTR = pr->m_mapRef_Type.find( tk );

            if(itTR != pr->m_mapRef_Type.end())
            {
                TypeRef& tr = itTR->second;

                missing = tr.m_name;

                switch(TypeFromToken( tr.m_scope ))
                {
                case mdtTypeRef:
                    {
                        TINYCLR_CHECK_HRESULT(ResolveTypeDef( pr, tr.m_scope, prDst, tdDst ));

                        for(TypeDefMapIter itTD = prDst->m_mapDef_Type.begin(); itTD != prDst->m_mapDef_Type.end(); itTD++)
                        {
                            TypeDef& td = itTD->second;

                            if(td.m_enclosingClass == tdDst->m_td && td.m_name == tr.m_name)
                            {
                                tdDst = &td;
                                TINYCLR_SET_AND_LEAVE(S_OK);
                            }
                        }
                    }
                    break;

                case mdtAssemblyRef:
                    {
                        TINYCLR_CHECK_HRESULT(ResolveAssemblyDef( pr, tr.m_scope, prDst ));

                        for(TypeDefMapIter itTD = prDst->m_mapDef_Type.begin(); itTD != prDst->m_mapDef_Type.end(); itTD++)
                        {
                            TypeDef& td = itTD->second;

                            if(IsNilToken(td.m_enclosingClass) && td.m_name == tr.m_name)
                            {
                                tdDst = &td;
                                TINYCLR_SET_AND_LEAVE(S_OK);
                            }
                        }
                    }
                    break;
                }
            }
        }
        break;

    case mdtTypeDef:
        {
            TypeDefMapIter itTD = pr->m_mapDef_Type.find( tk );

            if(itTD != pr->m_mapDef_Type.end())
            {
                TypeDef& td = itTD->second;

                prDst =  pr;
                tdDst = &td;
                TINYCLR_SET_AND_LEAVE(S_OK);
            }
        }
        break;
    }

    if(missing.size() == 0)
    {
        WCHAR rgBuffer[64]; swprintf( rgBuffer, ARRAYSIZE(rgBuffer), L"%08x", tk );

        missing = rgBuffer;
    }

    ErrorReporting::Print( pr->m_assemblyName.c_str(), NULL, TRUE, 0, L"Cannot resolve type %s from assembly %s!\n", missing.c_str(), pr->m_assemblyName.c_str() );
    TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    TINYCLR_NOCLEANUP();
}

//--//

bool MetaData::Collection::IsMethodToken( Parser* pr, mdToken tk )
{
    switch(TypeFromToken( tk ))
    {
    case mdtMemberRef:
        {
            MemberRefMapIter itMR = pr->m_mapRef_Member.find( tk );

            if(itMR != pr->m_mapRef_Member.end())
            {
                MemberRef& mr = itMR->second;

                return (mr.m_sig.m_type == mdtMethodDef);
            }
        }
        break;

    case mdtMethodDef:
        {
            MethodDefMapIter itMD = pr->m_mapDef_Method.find( tk );

            if(itMD != pr->m_mapDef_Method.end())
            {
                return true;
            }
        }
        break;
    }

    return false;
}

HRESULT MetaData::Collection::ResolveMethodDef( Parser* pr, mdToken tk, Parser*& prDst, MethodDef*& mdDst )
{
    TINYCLR_HEADER();

    std::wstring missing;

    prDst = NULL;
    mdDst = NULL;

    switch(TypeFromToken( tk ))
    {
    case mdtMemberRef:
        {
            MemberRefMapIter itMR = pr->m_mapRef_Member.find( tk );

            if(itMR != pr->m_mapRef_Member.end())
            {
                MemberRef& mr = itMR->second;

                if(mr.m_sig.m_type == mdtMethodDef)
                {
                    tk = mr.m_tr;

                    while(true)
                    {
                        TypeDef* tdDst;

                        TINYCLR_CHECK_HRESULT(ResolveTypeDef( pr, tk, prDst, tdDst ));

                        for(mdMethodDefListIter itMDL = tdDst->m_methods.begin(); itMDL != tdDst->m_methods.end(); itMDL++)
                        {
                            MethodDefMapIter itMD = prDst->m_mapDef_Method.find( *itMDL );

                            if(itMD != prDst->m_mapDef_Method.end())
                            {
                                MethodDef& md = itMD->second;

                                if(md.m_name == mr.m_name)
                                {
                                    if(md.m_method == mr.m_sig.m_sigMethod)
                                    {
                                        mdDst = &md;
                                        TINYCLR_SET_AND_LEAVE(S_OK);
                                    }
                                }
                            }
                        }

                        if(IsNilToken(tdDst->m_extends)) break;

                        pr = prDst;
                        tk = tdDst->m_extends;
                    }
                }

                missing = mr.m_name;
            }
        }
        break;

    case mdtMethodDef:
        {
            MethodDefMapIter itMD = pr->m_mapDef_Method.find( tk );

            if(itMD != pr->m_mapDef_Method.end())
            {
                MethodDef& md = itMD->second;

                prDst = pr;
                mdDst = &md;
                TINYCLR_SET_AND_LEAVE(S_OK);
            }
        }
        break;
    }

    if(missing.size() == 0)
    {
        WCHAR rgBuffer[64]; swprintf( rgBuffer, ARRAYSIZE(rgBuffer), L"%08x", tk );

        missing = rgBuffer;
    }

    ErrorReporting::Print( pr->m_assemblyName.c_str(), NULL, TRUE, 0, L"Cannot resolve method %s from assembly %s!\n", missing.c_str(), pr->m_assemblyName.c_str() );
    TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    TINYCLR_NOCLEANUP();
}

//--//

bool MetaData::Collection::IsFieldToken( Parser* pr, mdToken tk )
{
    switch(TypeFromToken( tk ))
    {
    case mdtMemberRef:
        {
            MemberRefMapIter itMR = pr->m_mapRef_Member.find( tk );

            if(itMR != pr->m_mapRef_Member.end())
            {
                MemberRef& mr = itMR->second;

                return (mr.m_sig.m_type == mdtFieldDef);
            }
        }
        break;

    case mdtFieldDef:
        {
            FieldDefMapIter itFD = pr->m_mapDef_Field.find( tk );

            if(itFD != pr->m_mapDef_Field.end())
            {
                return true;
            }
        }
        break;
    }

    return false;
}

HRESULT MetaData::Collection::ResolveFieldDef( Parser* pr, mdToken tk, Parser*& prDst, FieldDef*& fdDst )
{
    TINYCLR_HEADER();

    prDst = NULL;
    fdDst = NULL;

    switch(TypeFromToken( tk ))
    {
    case mdtMemberRef:
        {
            MemberRefMapIter itMR = pr->m_mapRef_Member.find( tk );

            if(itMR != pr->m_mapRef_Member.end())
            {
                MemberRef& mr = itMR->second;

                if(mr.m_sig.m_type == mdtFieldDef)
                {
                    TypeDef* tdDst;

                    TINYCLR_CHECK_HRESULT(ResolveTypeDef( pr, mr.m_tr, prDst, tdDst ));

                    for(mdFieldDefListIter itFDL = tdDst->m_fields.begin(); itFDL != tdDst->m_fields.end(); itFDL++)
                    {
                        FieldDefMapIter itFD = prDst->m_mapDef_Field.find( *itFDL );

                        if(itFD != prDst->m_mapDef_Field.end())
                        {
                            FieldDef& fd = itFD->second;

                            if(fd.m_name == mr.m_name)
                            {
                                if(fd.m_sig.m_sigField == mr.m_sig.m_sigField)
                                {
                                    fdDst = &fd;
                                    TINYCLR_SET_AND_LEAVE(S_OK);
                                }
                            }
                        }
                    }
                }
            }
        }
        break;

    case mdtFieldDef:
        {
            FieldDefMapIter itFD = pr->m_mapDef_Field.find( tk );

            if(itFD != pr->m_mapDef_Field.end())
            {
                FieldDef& fd = itFD->second;

                prDst = pr;
                fdDst = &fd;
                TINYCLR_SET_AND_LEAVE(S_OK);
            }
        }
        break;
    }

    ErrorReporting::Print( pr->m_assemblyName.c_str(), NULL, TRUE, 0, L"Cannot resolve field %s::%08x!", pr->m_assemblyName.c_str(), tk );
    TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    TINYCLR_NOCLEANUP();
}
