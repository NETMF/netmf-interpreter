////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"


////////////////////////////////////////////////////////////////////////////////////////////////////

static void SplitAtDelimiter( CLR_RT_StringVector& vec, const std::wstring& str, WCHAR ch )
{
    std::wstring::size_type iPos   = 0;
    std::wstring::size_type iStart = 0;

    while(1)
    {
        iPos = str.find( ch, iStart );
        if(iPos == std::wstring::npos)
        {
            vec.push_back( str.substr( iStart ) );
            break;
        }
        else
        {
            vec.push_back( str.substr( iStart, iPos - iStart ) );

            iStart = iPos + 1;
        }
    }
}

static void RemoveEmptyStrings( CLR_RT_StringVector& vec )
{
    CLR_RT_StringVectorIter it;

    while(true)
    {
        for(it=vec.begin(); it!=vec.end(); it++)
        {
            if(it->size() == 0) break;
        }

        if(it == vec.end()) break;

        vec.erase( it );
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::Parser::TokenToString( mdToken tk, std::wstring& str )
{
    WCHAR buffer[1024];

    str.clear();

    switch(TypeFromToken( tk ))
    {
    case mdtTypeRef:
        {
            TypeRefMapIter it = m_mapRef_Type.find( tk ); if(it == m_mapRef_Type.end()) break;
            TypeRef&       tr = it->second;

            if(IsNilToken( tr.m_scope ) == false)
            {
                if(TypeFromToken( tr.m_scope ) == mdtTypeRef)
                {
                    TokenToString( tr.m_scope, str );

                    str.append( L"." );
                }
                else
                {
                    TokenToString( tr.m_scope, str );
                }
            }

            str.append( tr.m_name );
            return;
        }

    case mdtTypeDef:
        {
            TypeDefMapIter it = m_mapDef_Type.find( tk ); if(it == m_mapDef_Type.end()) break;
            TypeDef&       td = it->second;

            if(IsNilToken( td.m_enclosingClass ) == false)
            {
                TokenToString( td.m_enclosingClass, str );

                str.append( L"+" );
            }

            str.append( td.m_name );
            return;
        }

    case mdtFieldDef:
        {
            FieldDefMapIter it = m_mapDef_Field.find( tk ); if(it == m_mapDef_Field.end()) break;
            FieldDef&       fd = it->second;

            if(IsNilToken( fd.m_td ) == false)
            {
                TokenToString( fd.m_td, str );

                str.append( L"::" );
            }

            str.append( fd.m_name );
            return;
        }

    case mdtMethodDef:
        {
            MethodDefMapIter it = m_mapDef_Method.find( tk ); if(it == m_mapDef_Method.end()) break;
            MethodDef&       md = it->second;

            if(IsNilToken( md.m_td ) == false)
            {
                TokenToString( md.m_td, str );

                str.append( L"::" );
            }

            str.append( md.m_name );
            return;
        }

    case mdtInterfaceImpl:
        {
            InterfaceImplMapIter it = m_mapDef_Interface.find( tk ); if(it == m_mapDef_Interface.end()) break;
            InterfaceImpl&       ii = it->second;

            std::wstring cls; TokenToString( ii.m_td , cls );
            std::wstring itf; TokenToString( ii.m_itf, itf );

            _snwprintf_s( buffer, ARRAYSIZE(buffer), MAXSTRLEN(buffer), L"[%s implements %s]", cls.c_str(), itf.c_str() );

            str = buffer;
            return;
        }

    case mdtMemberRef:
        {
            MemberRefMapIter it = m_mapRef_Member.find( tk ); if(it == m_mapRef_Member.end()) break;
            MemberRef&       mr = it->second;

            if(IsNilToken( mr.m_tr ) == false)
            {
                TokenToString( mr.m_tr, str );

                str.append( L"::" );
            }

            str.append( mr.m_name );
            return;
        }

    case mdtModuleRef:
        {
            ModuleRefMapIter it = m_mapRef_Module.find( tk ); if(it == m_mapRef_Module.end()) break;
            ModuleRef&       mr = it->second;

            _snwprintf_s( buffer, ARRAYSIZE(buffer), MAXSTRLEN(buffer), L"{%s}", mr.m_name.c_str() );

            str = buffer;
            return;
        }

    case mdtTypeSpec:
        {
            TypeSpecMapIter it = m_mapSpec_Type.find( tk ); if(it == m_mapSpec_Type.end()) break;
            TypeSpec&       ts = it->second;

            _snwprintf_s( buffer, ARRAYSIZE(buffer), L"[TypeSpec %08x]", ts.m_ts );

            str = buffer;
            return;
        }

    case mdtAssemblyRef:
        {
            AssemblyRefMapIter it = m_mapRef_Assembly.find( tk ); if(it == m_mapRef_Assembly.end()) break;
            AssemblyRef&       ar = it->second;

            _snwprintf_s( buffer, ARRAYSIZE(buffer), MAXSTRLEN(buffer), L"[%s]", ar.m_name.c_str() );

            str = buffer;
            return;
        }


    case mdtString:
        {
            UserStringMapIter it = m_mapDef_String.find( tk ); if(it == m_mapDef_String.end()) break;

            _snwprintf_s( buffer, ARRAYSIZE(buffer), MAXSTRLEN(buffer), L"'%s'", it->second.c_str() );

            str = buffer;
            return;
        }
    }

    _snwprintf_s( buffer, ARRAYSIZE(buffer), MAXSTRLEN(buffer), L"[%08x]", tk );

    str = buffer;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::Parser::DumpCompact( LPCWSTR szFileName )
{
    Dump_SetDevice( szFileName );

    fwprintf( m_output, L"# <Type Name>,<Token>,TYPE,<Extends>,<Enclosed>,<Flags>\n" );
    fwprintf( m_output, L"# <Type Name>,<Token>,FIELD,<TypeDef>,<Name> <Flags>,<Attributes>,<Signature>\n" );
    fwprintf( m_output, L"# <Type Name>,<Token>,METHOD,<TypeDef>,<Name>,<Flags>,<ImplFlags>\n" );
    fwprintf( m_output, L"# <Type Name>,<Token>,INTERFACE,<TypeDef>,<Interface>\n" );

    for(TypeDefMapIter it = m_mapDef_Type.begin(); it != m_mapDef_Type.end(); it++)
    {
        TypeDef& td = it->second;

        fwprintf( m_output, L"%s,%08x,TYPE,%08x,%08x,%08x\n", td.m_name.c_str(), it->second.m_td, td.m_extends, td.m_enclosingClass, td.m_flags );

        for(mdFieldDefListIter it1 = td.m_fields.begin(); it1 != td.m_fields.end(); it1++)
        {
            FieldDef& fd = m_mapDef_Field.find( *it1 )->second;

            if(fd.m_flags & fdLiteral) continue; // Don't generate constant fields.

            fwprintf( m_output, L"%s,%08x,FIELD,%08x,%s,%08x,%08x, ", td.m_name.c_str(), *it1, it->second.m_td, fd.m_name.c_str(), fd.m_flags, fd.m_attr );
            Dump_PrintSigForTypeSpec( fd.m_sig );
            fwprintf( m_output, L"\n" );
        }

        for(mdMethodDefListIter it2 = td.m_methods.begin(); it2 != td.m_methods.end(); it2++)
        {
            MethodDef& md = m_mapDef_Method.find( *it2 )->second;

            fwprintf( m_output, L"%s,%08x,METHOD,%08x,%s,%08x,%08x, ", td.m_name.c_str(), *it2, it->second.m_td, md.m_name.c_str(), md.m_flags, md.m_implFlags );
            Dump_PrintSigForMethod( md.m_method );
            fwprintf( m_output, L"\n" );
        }

        for(mdInterfaceImplListIter it3 = td.m_interfaces.begin(); it3 != td.m_interfaces.end(); it3++)
        {
            InterfaceImpl& ii = m_mapDef_Interface[*it3];

            fwprintf( m_output, L"%s,%08x,INTERFACE,%08x,%08x\n", td.m_name.c_str(), *it3, ii.m_td, ii.m_itf );
        }
    }


    Dump_CloseDevice();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::Parser::Dump_SetDevice( LPCWSTR szFileName )
{
    if(szFileName)
    {        
        if(_wfopen_s(&m_toclose, szFileName, L"w" ) != 0)
        {
            wprintf( L"Cannot open '%s' for writing!\n", szFileName );
        }
    }

    m_output = m_toclose ? m_toclose : stdout;
}

void MetaData::Parser::Dump_CloseDevice()
{
    if(m_toclose)
    {
        fclose( m_toclose ); m_toclose = NULL;
    }

    m_output = stdout;
}


void MetaData::Parser::Dump_PrintSigForType( TypeSignature& sig )
{
    switch(sig.m_opt)
    {
        case ELEMENT_TYPE_VOID      : fwprintf( m_output, L"VOID"                          ); break;
        case ELEMENT_TYPE_BOOLEAN   : fwprintf( m_output, L"BOOLEAN"                       ); break;
        case ELEMENT_TYPE_CHAR      : fwprintf( m_output, L"CHAR"                          ); break;
        case ELEMENT_TYPE_I1        : fwprintf( m_output, L"I1"                            ); break;
        case ELEMENT_TYPE_U1        : fwprintf( m_output, L"U1"                            ); break;
        case ELEMENT_TYPE_I2        : fwprintf( m_output, L"I2"                            ); break;
        case ELEMENT_TYPE_U2        : fwprintf( m_output, L"U2"                            ); break;
        case ELEMENT_TYPE_I4        : fwprintf( m_output, L"I4"                            ); break;
        case ELEMENT_TYPE_U4        : fwprintf( m_output, L"U4"                            ); break;
        case ELEMENT_TYPE_I8        : fwprintf( m_output, L"I8"                            ); break;
        case ELEMENT_TYPE_U8        : fwprintf( m_output, L"U8"                            ); break;
        case ELEMENT_TYPE_R4        : fwprintf( m_output, L"R4"                            ); break;
        case ELEMENT_TYPE_R8        : fwprintf( m_output, L"R8"                            ); break;
        case ELEMENT_TYPE_STRING    : fwprintf( m_output, L"STRING"                        ); break;
        case ELEMENT_TYPE_PTR       : fwprintf( m_output, L"PTR "                          ); break;
        case ELEMENT_TYPE_BYREF     : fwprintf( m_output, L"BYREF "                        ); break;
        case ELEMENT_TYPE_VALUETYPE : fwprintf( m_output, L"VALUETYPE [%08x]", sig.m_token ); break;
        case ELEMENT_TYPE_CLASS     : fwprintf( m_output, L"CLASS [%08x]"    , sig.m_token ); break;
        case ELEMENT_TYPE_ARRAY     : fwprintf( m_output, L"ARRAY!!"                       ); break;
        case ELEMENT_TYPE_TYPEDBYREF: fwprintf( m_output, L"TYPEDBYREF!!"                  ); break;
        case ELEMENT_TYPE_I         : fwprintf( m_output, L"I"                             ); break;
        case ELEMENT_TYPE_U         : fwprintf( m_output, L"U"                             ); break;
        case ELEMENT_TYPE_FNPTR     : fwprintf( m_output, L"FNPTR!!"                       ); break;
        case ELEMENT_TYPE_OBJECT    : fwprintf( m_output, L"OBJECT"                        ); break;
        case ELEMENT_TYPE_SZARRAY   : fwprintf( m_output, L"SZARRAY "                      ); break;

        case ELEMENT_TYPE_SENTINEL  : fwprintf( m_output, L"SENTINEL!!"                    ); break;
        case ELEMENT_TYPE_PINNED    : fwprintf( m_output, L"PINNED!!"                      ); break;
    }

    if(sig.m_sub) Dump_PrintSigForType( *sig.m_sub );
}

void MetaData::Parser::Dump_PrintSigForMethod( MethodSignature& sig )
{
    Dump_PrintSigForType( sig.m_retValue );

    fwprintf( m_output, L"(" );

    for(TypeSignatureIter it = sig.m_lstParams.begin(); it != sig.m_lstParams.end(); )
    {
        fwprintf( m_output, L" " );

        Dump_PrintSigForType( *it++ );

        fwprintf( m_output, (it == sig.m_lstParams.end()) ? L" " : L"," );
    }

    fwprintf( m_output, L")" );
}

void MetaData::Parser::Dump_PrintSigForLocalVar( LocalVarSignature& sig )
{
    fwprintf( m_output, L"{" );

    for(MetaData::TypeSignatureIter it = sig.m_lstVars.begin(); it != sig.m_lstVars.end(); )
    {
        fwprintf( m_output, L" " );

        Dump_PrintSigForType( *it++ );
        if(it != sig.m_lstVars.end()) fwprintf( m_output, L"," );
    }

    fwprintf( m_output, L"}" );
}

void MetaData::Parser::Dump_PrintSigForTypeSpec( TypeSpecSignature& sig )
{
    if(sig.m_type == mdtFieldDef)
    {
        Dump_PrintSigForType( sig.m_sigField );
    }

    if(sig.m_type == mdtSignature)
    {
        Dump_PrintSigForLocalVar( sig.m_sigLocal );
    }

    if(sig.m_type == mdtMethodDef)
    {
        Dump_PrintSigForMethod( sig.m_sigMethod );
    }
}

//--//

void MetaData::Parser::Dump_EnumAssemblyRefs()
{
    AssemblyRefMapIter it;

    for(it = m_mapRef_Assembly.begin(); it != m_mapRef_Assembly.end(); it++)
    {
        AssemblyRef& ar = it->second;

        fwprintf( m_output, L"AssemblyRefProps [%08x]: Flags: %08x '%s'\n", ar.m_ar, ar.m_flags, ar.m_name.c_str() );

        fwprintf( m_output, L"\n" );
    }
}

void MetaData::Parser::Dump_EnumModuleRefs()
{
    ModuleRefMapIter it;

    for(it = m_mapRef_Module.begin(); it != m_mapRef_Module.end(); it++)
    {
        ModuleRef& mr = it->second;

        fwprintf( m_output, L"ModuleRefProps [%08x]: '%s'\n", mr.m_mr, mr.m_name.c_str() );

        fwprintf( m_output, L"\n" );
    }
}

void MetaData::Parser::Dump_EnumTypeRefs()
{
    for(TypeRefMapIter it = m_mapRef_Type.begin(); it != m_mapRef_Type.end(); it++)
    {
        TypeRef& tr = it->second;

        fwprintf( m_output, L"TypeRefProps [%08x]: Scope: %08x '%s'\n", tr.m_tr, tr.m_scope, tr.m_name.c_str() );

        for(mdMemberRefListIter it2 = tr.m_lst.begin(); it2 != tr.m_lst.end(); it2++)
        {
            MemberRef& mr = m_mapRef_Member.find( *it2 )->second;

            fwprintf( m_output, L"    MemberRefProps [%08x]: '%s' [", mr.m_mr, mr.m_name.c_str() );
            Dump_PrintSigForTypeSpec( mr.m_sig );
            fwprintf( m_output, L"]\n" );
        }

        fwprintf( m_output, L"\n" );
    }
}

void MetaData::Parser::Dump_EnumTypeDefs( bool fNoByteCode )
{
    for(TypeDefMapIter it = m_mapDef_Type.begin(); it != m_mapDef_Type.end(); it++)
    {
        TypeDef& td = it->second;

        fwprintf( m_output, L"TypeDefProps [%08x]: Flags: %08x Extends: %08x Enclosed: %08x '%s'\n", td.m_td, td.m_flags, td.m_extends, td.m_enclosingClass, td.m_name.c_str() );

        for(mdFieldDefListIter it1 = td.m_fields.begin(); it1 != td.m_fields.end(); it1++)
        {
            FieldDef& fd = m_mapDef_Field.find( *it1 )->second;

            fwprintf( m_output, L"    FieldDefProps [%08x]: Attr: %08x Flags: %08x '%s' [", fd.m_fd, fd.m_attr, fd.m_flags, fd.m_name.c_str() );
            Dump_PrintSigForTypeSpec( fd.m_sig );
            fwprintf( m_output, L"]\n" );
        }

        for(mdMethodDefListIter it2 = td.m_methods.begin(); it2 != td.m_methods.end(); it2++)
        {
            MethodDef& md = m_mapDef_Method.find( *it2 )->second;

            fwprintf( m_output, L"    MethodDefProps [%08x]: Flags: %08x Impl: %08x RVA: %08x '%s' [", md.m_md, md.m_flags, md.m_implFlags, md.m_RVA, md.m_name.c_str() );
            Dump_PrintSigForMethod( md.m_method );
            fwprintf( m_output, L"]\n" );


            if(md.m_vars.m_lstVars.size())
            {
                fwprintf( m_output, L"        Locals %d: ", md.m_vars.m_lstVars.size() );
                Dump_PrintSigForLocalVar( md.m_vars );
                fwprintf( m_output, L"\n" );
            }

            {
                for(size_t i=0; i<md.m_byteCode.m_exceptions.size(); i++)
                {
                    ByteCode::LogicalExceptionBlock& leb = md.m_byteCode.m_exceptions[i];

                    if(leb.m_Flags == COR_ILEXCEPTION_CLAUSE_FILTER)
                    {
                        fwprintf( m_output, L"        EH: %02x %08x->%08x F:%08x %08x->%08x\n" ,
                            leb.m_Flags                              ,
                            leb.m_TryOffset                          ,
                            leb.m_TryOffset     + leb.m_TryLength    ,
                            leb.m_FilterOffset                       ,
                            leb.m_HandlerOffset                      ,
                            leb.m_HandlerOffset + leb.m_HandlerLength);
                    }
                    else
                    {
                        fwprintf( m_output, L"        EH: %02x %08x->%08x %08x->%08x %08x\n" ,
                            leb.m_Flags                              ,
                            leb.m_TryOffset                          ,
                            leb.m_TryOffset     + leb.m_TryLength    ,
                            leb.m_HandlerOffset                      ,
                            leb.m_HandlerOffset + leb.m_HandlerLength,
                            leb.m_ClassToken                         );
                    }
                }
            }

            if(md.m_VA && fNoByteCode == false)
            {
                COR_ILMETHOD_DECODER il( (const COR_ILMETHOD*)md.m_VA );

                if(il.Code)
                {
                    const CLR_UINT8* IP    = il.Code;
                    const CLR_UINT8* IPend = IP + il.GetCodeSize();

                    while(IP < IPend)
                    {
                        CLR_OPCODE op = CLR_ReadNextOpcode( IP );

                        fwprintf( m_output, L"           %-12S", c_CLR_RT_OpcodeLookup[op].m_name );

                        if(IsOpParamToken( c_CLR_RT_OpcodeLookup[op].m_opParam ))
                        {
                            CLR_UINT32 arg; TINYCLR_READ_UNALIGNED_UINT32( arg, IP );

                            fwprintf( m_output, L"[%08x]", arg );
                        }
                        else
                        {
                            IP = CLR_SkipBodyOfOpcode( IP, op );
                        }

                        fwprintf( m_output, L"\n" );
                    }
                }
            }

            fwprintf( m_output, L"\n" );
        }

        for(mdInterfaceImplListIter it3 = td.m_interfaces.begin(); it3 != td.m_interfaces.end(); it3++)
        {
            InterfaceImpl& ii = m_mapDef_Interface[*it3];

            fwprintf( m_output, L"    InterfaceImplProps [%08x]: Itf: %08x\n", ii.m_td, ii.m_itf );
        }

        fwprintf( m_output, L"\n" );
    }
}

void MetaData::Parser::Dump_EnumCustomAttributes()
{
    for(CustomAttributeMapIter itCA = m_mapDef_CustomAttribute.begin(); itCA != m_mapDef_CustomAttribute.end(); itCA++)
    {
        CustomAttribute& ca = itCA->second;

        fwprintf( m_output, L"Attribute: %s::[%08x %08x]\n", m_assemblyName.c_str(), ca.m_tkObj, ca.m_tkType );

        if(ca.m_valuesFixed.size())
        {
            fwprintf( m_output, L"Fixed Arguments:\n" );

            for(CustomAttribute::ValueListIter itF = ca.m_valuesFixed.begin(); itF != ca.m_valuesFixed.end(); itF++)
            {
                CustomAttribute::Value& v = *itF;

                fwprintf( m_output, L"%02x %016I64x %s\n", v.m_opt, v.m_numeric.u8, v.m_text.c_str() );
            }

            fwprintf( m_output, L"\n" );
        }

        if(ca.m_valuesVariable.size())
        {
            fwprintf( m_output, L"Named Arguments:\n" );

            for(CustomAttribute::ValueMapIter itV = ca.m_valuesVariable.begin(); itV != ca.m_valuesVariable.end(); itV++)
            {
                const CustomAttribute::Name& n = itV->first;
                CustomAttribute::Value&      v = itV->second;

                fwprintf( m_output, L"%c %s: %02x %016I64x %s\n", n.m_fField ? 'F' : 'P', n.m_text.c_str(), v.m_opt, v.m_numeric.u8, v.m_text.c_str() );
            }

            fwprintf( m_output, L"\n" );
        }
    }
}

void MetaData::Parser::Dump_EnumUserStrings()
{
    for(UserStringMapIter it = m_mapDef_String.begin(); it != m_mapDef_String.end(); it++)
    {
        fwprintf( m_output, L"UserString [%08x]: '%s'\n", it->first, it->second.c_str() );
    }
}

void MetaData::Parser::DumpSchema( LPCWSTR szFileName, bool fNoByteCode )
{
    Dump_SetDevice( szFileName );


    Dump_EnumAssemblyRefs    (             );
    Dump_EnumModuleRefs      (             );
    Dump_EnumTypeRefs        (             );

    Dump_EnumTypeDefs        ( fNoByteCode );
    Dump_EnumCustomAttributes(             );
    Dump_EnumUserStrings     (             );


    Dump_CloseDevice();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

void MetaData::Parser::Dump_ShowDependencies( mdToken tk, mdTokenSet& set, mdTokenSet& setAdd )
{
    std::wstring from; TokenToString( tk, from );

    for(mdTokenSetIter it = setAdd.begin(); it != setAdd.end(); it++)
    {
        if(set.find( *it ) == set.end())
        {
            std::wstring to; TokenToString( (mdToken)*it, to );

            fwprintf( m_output, L"%s -> %s\n", from.c_str(), to.c_str() );
        }
    }
}
