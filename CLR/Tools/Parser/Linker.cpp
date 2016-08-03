////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include <TinyCLR_Endian.h>

#include <crypto.h>

//--//

#if defined(DEBUG)

#define REPORT_NO_MEMORY() { if(IsDebuggerPresent()) { DebugBreak(); } TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_MEMORY); }
#define REPORT_FAILURE()   { if(IsDebuggerPresent()) { DebugBreak(); } return false                        ; }

#else

#define REPORT_NO_MEMORY() TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_MEMORY)
#define REPORT_FAILURE()   return false

#endif

//--//

void FindRange( CLR_UINT8 v, CLR_INT64& minValue, CLR_INT64& maxValue ) { minValue = 0; maxValue = 255; }

template<typename V> HRESULT CheckRange( CLR_INT64 value, V field, LPCWSTR szItem, LPCWSTR szKind )
{
    TINYCLR_HEADER();

    CLR_INT64 maxValue;
    CLR_INT64 minValue;

    FindRange( field, minValue, maxValue );

    if(value < minValue)
    {
        wprintf( L"Exceeded minimum %s (%I64d) of %s: %I64d\n", szItem, minValue, szKind, value );

        TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    }

    if(value > maxValue)
    {
        wprintf( L"Exceeded maximum %s (%I64d) of %s: %I64d\n", szItem, maxValue, szKind, value );

        TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
    }

    TINYCLR_NOCLEANUP();
}

template<typename T, typename V> HRESULT CheckRange( std::list< T >& lst, V field, LPCWSTR szItem, LPCWSTR szKind )
{
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(CheckRange( lst.size(), field, szItem, szKind ));

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

static HRESULT GetFullPath( const std::wstring& szRelative, std::wstring* pAbsolute, std::wstring* pFileName )
{
    TINYCLR_HEADER();

    WCHAR  buffer[MAX_PATH+1];
    WCHAR* pFile;
    int    cch;

    cch = ::GetFullPathNameW( szRelative.c_str(), MAXSTRLEN(buffer), buffer, &pFile );
    if(cch == 0 || cch >= ARRAYSIZE(buffer)) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    if(pAbsolute) *pAbsolute = buffer;
    if(pFileName) *pFileName = pFile;

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

LPCWSTR WatchAssemblyBuilder::ToHex( CLR_UINT32 u )
{
    static WCHAR rgBuffer[1024];

    swprintf( rgBuffer, ARRAYSIZE(rgBuffer), L"0x%08X", u );

    return rgBuffer;
}

void WatchAssemblyBuilder::ChangeExtensionOnFileName(std::wstring strFile, std::wstring& strFileNew, LPWSTR szExt)
{
    std::wstring::size_type i = strFile.find_last_of( '.' );

    if(i >= 0)
    {
        strFileNew  = strFile.substr( 0, i+1 );
        strFileNew += szExt;
    }
}

////////////////////////////////////////////////////////////////////////////////////////////////////

WatchAssemblyBuilder::Linker::ExceptionHandlerHierarchy::ExceptionHandlerHierarchy()
{
    m_data = NULL; // CLR_RECORD_EH*                        m_data;
                   // std::list<ExceptionHandlerHierarchy*> m_children;
}

WatchAssemblyBuilder::Linker::ExceptionHandlerHierarchy::ExceptionHandlerHierarchy( ExceptionHandlerHierarchy& ehh )
{
    m_data = NULL;

    *this = ehh;
}

WatchAssemblyBuilder::Linker::ExceptionHandlerHierarchy& WatchAssemblyBuilder::Linker::ExceptionHandlerHierarchy::operator=( const ExceptionHandlerHierarchy& ehh )
{
    Clear();

    if(ehh.m_data)
    {
        m_data = new CLR_RECORD_EH(); *m_data = *ehh.m_data;
    }

    for(std::list<ExceptionHandlerHierarchy*>::const_iterator it = ehh.m_children.begin(); it != ehh.m_children.end(); it++)
    {
        ExceptionHandlerHierarchy* orig  = *it;
        ExceptionHandlerHierarchy* child = new ExceptionHandlerHierarchy( *orig );

        m_children.push_back( child );
    }

    return *this;
}

WatchAssemblyBuilder::Linker::ExceptionHandlerHierarchy::~ExceptionHandlerHierarchy()
{
    Clear();
}

WatchAssemblyBuilder::Linker::ExceptionHandlerHierarchy::ExceptionHandlerHierarchy( const CLR_RECORD_EH& eh )
{
    m_data = new CLR_RECORD_EH; *m_data = eh;
}

void WatchAssemblyBuilder::Linker::ExceptionHandlerHierarchy::Clear()
{
    delete m_data; m_data = NULL;

    for(std::list<ExceptionHandlerHierarchy*>::iterator it = m_children.begin(); it != m_children.end(); it++)
    {
        delete *it;
    }
    m_children.clear();
}

//--//

bool WatchAssemblyBuilder::Linker::ExceptionHandlerHierarchy::FindMaxCoverage( CLR_OFFSET& start, CLR_OFFSET& end ) const
{
    bool fGot;

    if(m_data)
    {
        start = m_data->tryStart;
        end   = m_data->tryEnd;

        fGot = true;
    }
    else
    {
        fGot = false;
    }

    for(std::list<ExceptionHandlerHierarchy*>::const_iterator it = m_children.begin(); it != m_children.end(); it++)
    {
        ExceptionHandlerHierarchy* child = *it;
        CLR_OFFSET                 childStart;
        CLR_OFFSET                 childEnd;

        if(child->FindMaxCoverage( childStart, childEnd ))
        {
            if(fGot)
            {
                if(start > childStart) start = childStart;
                if(end   < childEnd  ) end   = childEnd  ;
            }
            else
            {
                start = childStart;
                end   = childEnd  ;
                fGot  = true;
            }
        }
    }

    return fGot;
}

//--//

void WatchAssemblyBuilder::Linker::ExceptionHandlerHierarchy::Queue( const CLR_RECORD_EH& eh )
{
    Queue( new ExceptionHandlerHierarchy( eh ) );
}

void WatchAssemblyBuilder::Linker::ExceptionHandlerHierarchy::Queue( ExceptionHandlerHierarchy* newEhh )
{
    CLR_OFFSET                                      start = newEhh->m_data->tryStart;
    CLR_OFFSET                                      end   = newEhh->m_data->tryEnd;
    std::list<ExceptionHandlerHierarchy*>::iterator it;

    for(it = m_children.begin(); it != m_children.end(); )
    {
        ExceptionHandlerHierarchy* child = *it;
        CLR_OFFSET                 childStart;
        CLR_OFFSET                 childEnd;

        if(child->FindMaxCoverage( childStart, childEnd ))
        {
            //
            // Is child covered? Swap the two.
            //
            if(start <= childStart && end >= childEnd)
            {
                newEhh->Queue( child );

                m_children.erase( it );
                it = m_children.begin();
                continue;
            }

            //
            // Does child cover the new block? Recurse.
            //
            if(childStart <= start && childEnd >= end)
            {
                child->Queue( newEhh );
                return;
            }

            if(childStart > end)
            {
                break;
            }

            it++;
        }
    }

    m_children.insert( it, newEhh );
}

HRESULT WatchAssemblyBuilder::Linker::ExceptionHandlerHierarchy::GenerateOutput( CQuickRecord<CLR_RECORD_EH>& tbl )
{
    TINYCLR_HEADER();

    for(std::list<ExceptionHandlerHierarchy*>::iterator it = m_children.begin(); it != m_children.end(); it++)
    {
        ExceptionHandlerHierarchy* ehh = *it;

        TINYCLR_CHECK_HRESULT(ehh->GenerateOutput( tbl ));
    }

    if(m_data)
    {
        //wprintf( L"%04x %04x %04x %04x\n", m_data->classToken, m_data->tryStart, m_data->tryEnd, m_data->handlerStart );

        CLR_RECORD_EH* dstEH = tbl.Alloc( 1 ); if(dstEH == NULL) REPORT_NO_MEMORY();

        *dstEH = *m_data;
    }

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

WatchAssemblyBuilder::Linker::Linker()
{
    m_pr  = NULL;
	m_fBigEndianTarget = false;
}

WatchAssemblyBuilder::Linker::~Linker()
{
}

void WatchAssemblyBuilder::Linker::Clean()
{
    m_tableAssemblyRef     .Destroy();   // CQuickRecord<CLR_RECORD_ASSEMBLYREF> m_tableAssemblyRef ;
    m_tableTypeRef         .Destroy();   // CQuickRecord<CLR_RECORD_TYPEREF    > m_tableTypeRef     ;
    m_tableFieldRef        .Destroy();   // CQuickRecord<CLR_RECORD_FIELDREF   > m_tableFieldRef    ;
    m_tableMethodRef       .Destroy();   // CQuickRecord<CLR_RECORD_METHODREF  > m_tableMethodRef   ;
    m_tableTypeDef         .Destroy();   // CQuickRecord<CLR_RECORD_TYPEDEF    > m_tableTypeDef     ;
    m_tableFieldDef        .Destroy();   // CQuickRecord<CLR_RECORD_FIELDDEF   > m_tableFieldDef    ;
    m_tableMethodDef       .Destroy();   // CQuickRecord<CLR_RECORD_METHODDEF  > m_tableMethodDef   ;
    m_tableAttribute       .Destroy();   // CQuickRecord<CLR_RECORD_ATTRIBUTE  > m_tableAttribute   ;
    m_tableTypeSpec        .Destroy();   // CQuickRecord<CLR_RECORD_TYPESPEC   > m_tableTypeSpec    ;
    m_tableResource        .Destroy();   // CQuickRecord<CLR_RECORD_RESOURCE   > m_tableResource    ;
    m_tableResourceFile    .Destroy();   // CQuickRecord<CLR_RECORD_RESOURCE_FILE> m_tableResourceFile;
    m_tableResourceData    .Destroy();   // CQuickRecord<BYTE                  > m_tableResourceData;
    m_tableString          .Destroy();   // CQuickRecord<CHAR                  > m_tableString      ;
    m_tableSignature       .Destroy();   // CQuickRecord<BYTE                  > m_tableSignature   ;
    m_tableByteCode        .Destroy();   // CQuickRecord<BYTE                  > m_tableByteCode    ;
                                         //
                                         // std::set<std::string>                m_collectUniqueStrings;
                                         //
                                         // std::map<std::string,CLR_OFFSET>     m_lookupStringsConst;
    m_lookupStrings        .clear();     // std::map<std::string,CLR_OFFSET>     m_lookupStrings;
    m_lookupIDs            .clear();     // MetaData::mdTokenMap                 m_lookupIDs;
    m_posString            .clear();     // MetaData::mdTokenMap                 m_posString;
                                         //
    m_setAttributes_Types  .clear();     // MetaData::mdTokenSet                 m_setAttributes_Types;
    m_setAttributes_Fields .clear();     // MetaData::mdTokenSet                 m_setAttributes_Fields;
    m_setAttributes_Methods.clear();     // MetaData::mdTokenSet                 m_setAttributes_Methods;
                                         //
                                         // MetaData::Parser*                    m_pr;
                                         // BYTE                                 m_tmpSig[1024];
                                         // BYTE*                                m_tmpSigPtr;
                                         // BYTE*                                m_tmpSigEnd;
}

//--//

bool WatchAssemblyBuilder::Linker::AllocString( const std::string& str, CLR_STRING& idx, bool fUser )
{
    if(fUser == false)
    {
        // Don't collect generated strings
        if((str.find("$$method0x") != 0) &&
           (str.find("<PrivateImplementationDetails>{") != 0) &&
           (str.find("__StaticArrayInitTypeSize=") != 0))
        {
          m_collectUniqueStrings.insert( str );
        }

        if(m_lookupStringsConst.find( str ) != m_lookupStringsConst.end())
        {
            idx = m_lookupStringsConst[str];
            return true;
        }
    }

    if(m_lookupStrings.find( str ) != m_lookupStrings.end())
    {
        idx = m_lookupStrings[str];
    }
    else
    {
        int num     = (int)m_posString.size();
        int pos     = (int)m_tableString.GetPos();      
        int bufSize = (int)str.size()+1;
        CHAR* buf   = m_tableString.Alloc( bufSize ); if(buf == NULL) REPORT_FAILURE();

        if(pos > 0x7FFF)
        {
            wprintf( L"Exceeded maximum size of string table: %d\n", pos );

            REPORT_FAILURE();
        }

        strcpy_s( buf, bufSize, str.c_str() );

        idx = (CLR_STRING)pos;

        m_lookupStrings[str] = pos;
        m_posString    [num] = pos;
    }

    return true;
}

bool WatchAssemblyBuilder::Linker::AllocString( const std::wstring& str, CLR_STRING& idx, bool fUser )
{
    std::string dst;

    CLR_RT_UnicodeHelper::ConvertToUTF8( str, dst );

    return AllocString( dst, idx, fUser );
}

bool WatchAssemblyBuilder::Linker::AllocString( const std::wstring& str, CLR_STRING& name, CLR_STRING& nameSpace )
{
    std::wstring::size_type i = str.find_last_of( '.' );

    if(i != str.npos)
    {
        if(!AllocString( str.substr( 0  , i ), nameSpace, false )) return false;
        if(!AllocString( str.substr( i+1    ), name     , false )) return false;
    }
    else
    {
        nameSpace = 0;

        if(!AllocString( str, name, false )) return false;
    }

    return true;
}

bool WatchAssemblyBuilder::Linker::AllocString( const std::string& strName, const std::string& strNameSpace, CLR_STRING& name, CLR_STRING& nameSpace )
{
    if(strNameSpace.size() > 0)
    {
        if(!AllocString( strNameSpace, nameSpace, false )) return false;
        if(!AllocString( strName     , name     , false )) return false;
    }
    else
    {
        nameSpace = 0;

        if(!AllocString( strName, name, false )) return false;
    }

    return true;
}

HRESULT WatchAssemblyBuilder::Linker::SaveUniqueStrings( const std::wstring& file )
{
    TINYCLR_HEADER();

    CLR_RT_Buffer buffer;
    CLR_UINT8*    ptr;
        
    if(m_collectUniqueStrings.size() > 0)
    {
        for(int pass=0; pass<2; pass++)
        {
            size_t tot = 0;

            for(std::set<std::string>::iterator it = m_collectUniqueStrings.begin(); it != m_collectUniqueStrings.end(); it++)
            {
                std::string            str = *it;
                std::string::size_type len = str.size();

                if(pass == 1)
                {
                    memcpy( ptr, str.c_str(), len + 1 ); ptr += len + 1;
                }

                tot += len + 1;
            }

            if(pass == 0)
            {
                buffer.resize( tot );

                ptr = &buffer[0];
            }
        }
    }

    TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::SaveFile( file.c_str(), buffer ));

    TINYCLR_NOCLEANUP();
}

HRESULT WatchAssemblyBuilder::Linker::LoadUniqueStrings( const std::wstring& file )
{
    TINYCLR_HEADER();

    CLR_RT_Buffer buffer;

    TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( file.c_str(), buffer ));

    if(buffer.size() > 0)
    {
        CLR_UINT8* ptr    = &buffer[0            ];

        CLR_UINT8* ptrEnd = &buffer[buffer.size() - 1];

        if(ptrEnd[0] == 0) // It has to be zero terminated!!!
        {
            while(ptr <= ptrEnd)
            {
                m_collectUniqueStrings.insert( std::string( (LPCSTR)ptr ) ); ptr += hal_strlen_s( (LPCSTR)ptr ) + 1;
            }
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT WatchAssemblyBuilder::Linker::DumpUniqueStrings( const std::wstring& file )
{
    TINYCLR_HEADER();

    std::set<std::string>::iterator it;
    int                             i;
    size_t                          tot = 0;
    std::string                     total;
    FILE*                           stream;

    if(_wfopen_s(&stream, file.c_str(), L"w" ) != 0)
    {
        wprintf( L"Cannot open '%s'!\n", file.c_str() );
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    fprintf( stream, "const size_t c_CLR_StringTable_Size = %d;\n\n", m_collectUniqueStrings.size() );

    fprintf( stream, "const CLR_STRING c_CLR_StringTable_Lookup[] = \n{\n" );
    for(i = 0, it = m_collectUniqueStrings.begin(); it != m_collectUniqueStrings.end(); it++, i++)
    {
        fprintf( stream, "    /* %04X */ %d,\n", i, tot );

        tot += it->length() + 1;
    }
    fprintf( stream, "};\n\n" );

    fprintf( stream, "const char c_CLR_StringTable_Data[] =\n" );
    for(i = 0, it = m_collectUniqueStrings.begin(); it != m_collectUniqueStrings.end(); it++, i++)
    {
        std::string            str = *it;
        std::string::size_type pos = 0;

        while(true)
        {
            pos = str.find( '"', pos ); if(pos == str.npos) break;

            str.insert( pos, "\\" ); pos += 2;
        }

        fprintf( stream, " /* %04X */ \"%s\\0\"\n", i, str.c_str() );
    }
    fprintf( stream, ";\n" );

    fclose( stream );

    TINYCLR_NOCLEANUP();
}

void WatchAssemblyBuilder::Linker::LoadGlobalStrings()
{
    CLR_RT_Assembly::InitString( m_lookupStringsConst );
}

CLR_DataType WatchAssemblyBuilder::Linker::MapElementTypeToDataType( CorElementType et )
{
    CLR_DataType dt = CLR_RT_TypeSystem::MapElementTypeToDataType( et );

    if(dt == DATATYPE_FIRST_INVALID)
    {
        wprintf( L"Unsupported ElementType: %02X\n", et );
    }

    return dt;
}

//--//

bool WatchAssemblyBuilder::Linker::CheckDuplicateOrAppend( CLR_IDX& idx, BYTE* dst, size_t dstLen, const BYTE* src, size_t srcLen, size_t align, LPCWSTR szText )
{
    CLR_UINT32 offset;
    bool       fRes;

    fRes = CheckDuplicateOrAppend( offset, dst, dstLen, src, srcLen, align, szText );

    idx = (CLR_IDX)offset;

    return fRes;
}

bool WatchAssemblyBuilder::Linker::CheckDuplicateOrAppend( CLR_UINT32& offset, BYTE* dst, size_t dstLen, const BYTE* src, size_t srcLen, size_t align, LPCWSTR szText )
{
    const BYTE* dstOrig    = dst;
    size_t      dstLenOrig = dstLen;

    while(dstLen >= srcLen)
    {
        if(*dst == *src)
        {
            if(memcmp( dst, src, srcLen ) == 0)
            {
                offset = (CLR_UINT32)(dst - dstOrig);

                //wprintf( L"Duplicate: %s %d %d %08x %08x\r\n", szText, offset, dstLenOrig, dst, dstOrig );
                //CLR_Debug::DumpBufferV( src, srcLen, false, true, NULL, printf );

                return true;
            }
        }

        dst    += align;
        dstLen -= align;
    }

    offset = (CLR_UINT32)dstLenOrig;

    //printf( "Unique: %d\r\n", idx );
    //CLR_Debug::DumpBufferV( src, srcLen, false, true, NULL, printf );

    return false;
}

//--//--//

void WatchAssemblyBuilder::Linker::PrepareSignature()
{
    m_tmpSigPtr = m_tmpSig;
    m_tmpSigEnd = m_tmpSig + (ARRAYSIZE(m_tmpSig) / 2);
}

//--//

bool WatchAssemblyBuilder::Linker::GenerateSignature( MetaData::TypeSignature* sig )
{
    // If there is modifier on type record of local variable, 
    // we put it before type of local variable.
    if ( sig->m_optTypeModifier != ELEMENT_TYPE_END )
    {
       *m_tmpSigPtr++ = sig->m_optTypeModifier;
    }
    
    // Writes the type of local varialble to the function signature
    *m_tmpSigPtr++ = MapElementTypeToDataType( sig->m_opt );

    // For variables that have sub type, now we write sub type to the steam.
    switch(sig->m_opt)         
    {
      //case ELEMENT_TYPE_PTR       : return GenerateSignature     ( sig->m_sub   );
        case ELEMENT_TYPE_BYREF     : return GenerateSignature     ( sig->m_sub   );
        case ELEMENT_TYPE_VALUETYPE : return GenerateSignatureToken( sig->m_token );
        case ELEMENT_TYPE_CLASS     : return GenerateSignatureToken( sig->m_token );
        case ELEMENT_TYPE_FNPTR     : return GenerateSignatureToken( sig->m_token );
        case ELEMENT_TYPE_SZARRAY   : return GenerateSignature     ( sig->m_sub   );
    }

    return (m_tmpSigPtr < m_tmpSigEnd);
}

//--//

bool WatchAssemblyBuilder::Linker::GenerateSignature( MetaData::CustomAttribute& ca )
{
    MetaData::CustomAttribute::Writer writer( this, m_tmpSigPtr, m_tmpSigEnd );

    for(MetaData::CustomAttribute::ValueListIter itF = ca.m_valuesFixed.begin(); itF != ca.m_valuesFixed.end(); itF++)
    {
        MetaData::CustomAttribute::Value& v = *itF;

        if(!v.Emit( writer )) REPORT_FAILURE();
    }

    if(!writer.Write( (CLR_UINT16)ca.m_valuesVariable.size() )) REPORT_FAILURE();

    for(MetaData::CustomAttribute::ValueMapIter itV = ca.m_valuesVariable.begin(); itV != ca.m_valuesVariable.end(); itV++)
    {
        const MetaData::CustomAttribute::Name& n = itV->first;
        MetaData::CustomAttribute::Value&      v = itV->second;

        if(!writer.Write( n.m_fField ? SERIALIZATION_TYPE_FIELD : SERIALIZATION_TYPE_PROPERTY )) REPORT_FAILURE();
        if(!writer.Write( n.m_text                                                            )) REPORT_FAILURE();

        if(!v.Emit( writer )) REPORT_FAILURE();
    }

    return true;
}

//--//

bool WatchAssemblyBuilder::Linker::GenerateSignatureData8( CLR_UINT8 val )
{
    *m_tmpSigPtr++ = val;

    return (m_tmpSigPtr < m_tmpSigEnd);
}

bool WatchAssemblyBuilder::Linker::GenerateSignatureData32( CLR_UINT32 val )
{
    if(!CLR_CompressData( val, m_tmpSigPtr )) return false;

    return (m_tmpSigPtr < m_tmpSigEnd);
}

//--//

bool WatchAssemblyBuilder::Linker::GenerateSignatureToken( mdToken tk )
{
    if(MetaData::IsTokenPresent( m_lookupIDs, tk ) == false) REPORT_FAILURE();

    CLR_UINT32 token = CLR_DataFromTk( m_lookupIDs[tk] );

    switch(TypeFromToken( tk ))
    {
    case mdtTypeDef : token = token << 2 | 0x00; break;
    case mdtTypeRef : token = token << 2 | 0x01; break;
    case mdtTypeSpec: token = token << 2 | 0x02; break;
    default         : REPORT_FAILURE();
    }

    return GenerateSignatureData32( token );
}


//--//

bool WatchAssemblyBuilder::Linker::FlushSignature( CLR_SIG& idx, const BYTE* ptr, int len )
{
    if(CheckDuplicateOrAppend( idx, (BYTE*)m_tableSignature.Ptr(), m_tableSignature.Size(), ptr, len, sizeof(CLR_UINT8), L"Signature" )) return true;

    BYTE* res = m_tableSignature.Alloc( len ); if(res == NULL) REPORT_FAILURE();
    memcpy( res, ptr, len );

    if(m_tableSignature.Size() > 0x7FFF)
    {
        wprintf( L"Exceeded maximum size of signature table: %d\n", m_tableSignature.Size() );
        return false;
    }

    return true;
}

bool WatchAssemblyBuilder::Linker::FlushSignature( CLR_SIG& idx )
{
    return FlushSignature( idx, m_tmpSig, (int)(m_tmpSigPtr - m_tmpSig) );
}

//--//

bool WatchAssemblyBuilder::Linker::AllocSignatureForField( MetaData::TypeSignature* sig, CLR_SIG& idx )
{
    PrepareSignature();

    *m_tmpSigPtr++ = IMAGE_CEE_CS_CALLCONV_FIELD;
    if(!GenerateSignature( sig )) return false;

    return FlushSignature( idx );
}


//--//

bool WatchAssemblyBuilder::Linker::AllocSignatureForMethod( MetaData::MethodSignature* sig, CLR_SIG& idx )
{
    PrepareSignature();

    *m_tmpSigPtr++ = sig->m_flags;

    if(!GenerateSignatureData8( (CLR_UINT8)sig->m_lstParams.size() )) return false;

    if(!GenerateSignature( &sig->m_retValue )) return false;

    for(MetaData::TypeSignatureIter it = sig->m_lstParams.begin(); it != sig->m_lstParams.end(); it++)
    {
        if(!GenerateSignature( &(*it) )) return false;
    }

    return FlushSignature( idx );
}


//--//

bool WatchAssemblyBuilder::Linker::AllocSignatureForAttribute( MetaData::CustomAttribute& ca, CLR_SIG& idx )
{
    PrepareSignature();

    if(!GenerateSignature( ca )) return false;

    //CLR_Debug::DumpBufferV( m_tmpSig, (int)(m_tmpSigPtr - m_tmpSig), false, true, NULL, printf );

    return FlushSignature( idx );
}

//--//

bool WatchAssemblyBuilder::Linker::AllocSignatureForTypeSpec( MetaData::TypeSignature* sig, CLR_SIG& idx )
{
    PrepareSignature();

    if(!GenerateSignature( sig )) return false;

    return FlushSignature( idx );
}

//--//

bool WatchAssemblyBuilder::Linker::AllocSignatureForLocalVar( MetaData::LocalVarSignature& sig, CLR_SIG& idx )
{
    if(sig.m_lstVars.size() == 0) { idx = CLR_EmptyIndex; return true; }

    PrepareSignature();

    for(MetaData::TypeSignatureIter it = sig.m_lstVars.begin(); it != sig.m_lstVars.end(); it++)
    {
        MetaData::TypeSignature& sig2 = *it;

        if(!GenerateSignature( &sig2 )) return false;
    }

    return FlushSignature( idx );
}

//--//

bool WatchAssemblyBuilder::Linker::AllocSignatureForInterfaces( MetaData::mdInterfaceImplList& sig, CLR_SIG& idx )
{
    if(sig.size() == 0) { idx = CLR_EmptyIndex; return true; }

    PrepareSignature();

    if(!GenerateSignatureData8( (CLR_UINT8)sig.size() )) return false;

    for(MetaData::mdInterfaceImplListIter it = sig.begin(); it != sig.end(); it++)
    {
        MetaData::InterfaceImpl& ii = m_pr->m_mapDef_Interface[*it];

        if(!GenerateSignatureToken( ii.m_itf )) return false;
    }

    return FlushSignature( idx );
}

//--//

HRESULT WatchAssemblyBuilder::Linker::ValidateTypeRefOrDef( mdToken tk, bool fAsInterface )
{
    TINYCLR_HEADER();

    if(IsNilToken(tk))
    {
        if(fAsInterface == false) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL); // Only interfaces don't extend anything.

        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    TINYCLR_SET_AND_LEAVE(MetaData::IsTokenPresent( m_lookupIDs, tk ) ? S_OK : S_FALSE);

    TINYCLR_NOCLEANUP();
}

//--//

CLR_IDX WatchAssemblyBuilder::Linker::EncodeTypeRefOrDef( mdToken tk )
{
    if(IsNilToken(tk))
    {
        return CLR_EmptyIndex;
    }

    CLR_IDX val = m_lookupIDs[tk];

    return (TypeFromToken( tk ) == mdtTypeRef) ? val | 0x8000 : val;
}

//--//

CLR_IDX WatchAssemblyBuilder::Linker::EncodeToken( mdToken tk )
{
    if(IsNilToken(tk))
    {
        return CLR_EmptyIndex;
    }

    return m_lookupIDs[tk];
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT WatchAssemblyBuilder::Linker::Process( MetaData::Parser& pr )
{
    TINYCLR_HEADER();

    Clean();

    m_pr = &pr;

    for(MetaData::CustomAttributeMapIter itCA = m_pr->m_mapDef_CustomAttribute.begin(); itCA != m_pr->m_mapDef_CustomAttribute.end(); itCA++)
    {
        MetaData::CustomAttribute& ca = itCA->second;

        switch(TypeFromToken( ca.m_tkObj ))
        {
        case mdtTypeDef  : m_setAttributes_Types  .insert( ca.m_tkObj ); break;
        case mdtFieldDef : m_setAttributes_Fields .insert( ca.m_tkObj ); break;
        case mdtMethodDef: m_setAttributes_Methods.insert( ca.m_tkObj ); break;
        }
    }

    //
    // First string of heap is always the null string!!
    //
    {
        CLR_STRING idx;

        if(!AllocString( std::wstring(), idx, true )) REPORT_NO_MEMORY();
    }

    //--//

    {
        MetaData::mdTypeDefList order;

        TINYCLR_CHECK_HRESULT(ProcessAssemblyRef(       ));
        TINYCLR_CHECK_HRESULT(ProcessTypeRef    (       ));
        TINYCLR_CHECK_HRESULT(ProcessMemberRef  (       ));
        TINYCLR_CHECK_HRESULT(ProcessTypeDef    ( order ));
        TINYCLR_CHECK_HRESULT(ProcessTypeSpec   (       ));
        TINYCLR_CHECK_HRESULT(ProcessAttribute  (       ));
        TINYCLR_CHECK_HRESULT(ProcessResource   (       ));
        TINYCLR_CHECK_HRESULT(ProcessUserString (       ));

        for(MetaData::mdTypeDefListIter itOrder = order.begin(); itOrder != order.end(); itOrder++)
        {
            TINYCLR_CHECK_HRESULT(ProcessTypeDef_ByteCode( (mdTypeDef)*itOrder ));
        }
    }

    TINYCLR_NOCLEANUP();
}


//--//

HRESULT WatchAssemblyBuilder::Linker::ProcessAssemblyRef()
{
    TINYCLR_HEADER();

    for(MetaData::AssemblyRefMapIter it = m_pr->m_mapRef_Assembly.begin(); it != m_pr->m_mapRef_Assembly.end(); it++)
    {
        mdToken                tk = it->first;
        MetaData::AssemblyRef& ar = it->second;

        m_lookupIDs[tk] = CLR_TkFromType( TBL_AssemblyRef, (int)m_tableAssemblyRef.GetPos() );
        CLR_RECORD_ASSEMBLYREF* dst = m_tableAssemblyRef.Alloc( 1 ); if(dst == NULL) REPORT_NO_MEMORY();

        if(!AllocString( ar.m_name, dst->name, false )) REPORT_NO_MEMORY();

        dst->version = ar.m_version;
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT WatchAssemblyBuilder::Linker::ProcessTypeRef()
{
    TINYCLR_HEADER();

    MetaData::TypeRefMapIter    it;
    MetaData::mdTypeRefListIter itOrder;
    MetaData::mdTypeRefList     order;
    MetaData::mdTokenSet        resolved;

    //
    // We iterate through all the TypeRefs, resolving dependencies among them.
    //
    // This is good, because the layout will be ordered and no multiple passes will be required on the runtime end.
    //
    for(it = m_pr->m_mapRef_Type.begin(); it != m_pr->m_mapRef_Type.end(); it++)
    {
        TINYCLR_CHECK_HRESULT(ResolveTypeRef( it->second, order, resolved ));
    }


    //
    // Now we walk through the 'order' list, no forward dependencies left.
    //

    //
    // First, create all the records.
    //
    for(itOrder = order.begin(); itOrder != order.end(); itOrder++)
    {
        MetaData::TypeRef& tr = m_pr->m_mapRef_Type[ *itOrder ];

        m_lookupIDs[tr.m_tr] = CLR_TkFromType( TBL_TypeRef, (CLR_UINT32)m_tableTypeRef.GetPos() );

        CLR_RECORD_TYPEREF* dst = m_tableTypeRef.Alloc( 1 ); if(dst == NULL) REPORT_NO_MEMORY();
    }

    //
    // Then, populate them.
    //
    for(itOrder = order.begin(); itOrder != order.end(); itOrder++)
    {
        MetaData::TypeRef& tr = m_pr->m_mapRef_Type[ *itOrder ];

        CLR_RECORD_TYPEREF* dst = m_tableTypeRef.GetRecordAt( CLR_DataFromTk( m_lookupIDs[tr.m_tr] ) );

        if(!AllocString( tr.m_name, dst->name, dst->nameSpace )) REPORT_NO_MEMORY();

        dst->scope = EncodeTypeRefOrDef( tr.m_scope );
    }

    TINYCLR_NOCLEANUP();
}


//--//

HRESULT WatchAssemblyBuilder::Linker::ProcessMemberRef()
{
    TINYCLR_HEADER();

    MetaData::MemberRefMapIter it;

    for(it = m_pr->m_mapRef_Member.begin(); it != m_pr->m_mapRef_Member.end(); it++)
    {
        mdToken              tk = it->first;
        MetaData::MemberRef& mr = it->second;
        CLR_UINT32           tk2;

        if(it->second.m_sig.m_type == mdtFieldDef)
        {
            m_lookupIDs[tk] = CLR_TkFromType( TBL_FieldRef, (CLR_UINT32)m_tableFieldRef.GetPos() );

            CLR_RECORD_FIELDREF* dst = m_tableFieldRef.Alloc( 1 ); if(dst == NULL) REPORT_NO_MEMORY();

            if(!AllocString           (  mr.m_name          , dst->name, false )) REPORT_NO_MEMORY();
            if(!AllocSignatureForField( &mr.m_sig.m_sigField, dst->sig         )) REPORT_NO_MEMORY();

            tk2 = m_lookupIDs[ mr.m_tr ]; if(CLR_TypeFromTk(tk2) != TBL_TypeRef) REPORT_NO_MEMORY();

            dst->container = CLR_DataFromTk( tk2 );
        }

        if(it->second.m_sig.m_type == mdtMethodDef)
        {
            m_lookupIDs[tk] = CLR_TkFromType( TBL_MethodRef, (CLR_UINT32)m_tableMethodRef.GetPos() );

            CLR_RECORD_METHODREF* dst = m_tableMethodRef.Alloc( 1 ); if(dst == NULL) REPORT_NO_MEMORY();

            if(!AllocString            (  mr.m_name           , dst->name, false )) REPORT_NO_MEMORY();
            if(!AllocSignatureForMethod( &mr.m_sig.m_sigMethod, dst->sig         )) REPORT_NO_MEMORY();

            tk2 = m_lookupIDs[ mr.m_tr ]; if(CLR_TypeFromTk(tk2) != TBL_TypeRef) REPORT_NO_MEMORY();

            dst->container = CLR_DataFromTk( tk2 );
        }
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT WatchAssemblyBuilder::Linker::ProcessTypeDef( MetaData::mdTypeDefList& order )
{
    TINYCLR_HEADER();

    MetaData::TypeDefMapIter    it;
    MetaData::mdTypeDefListIter itOrder;
    MetaData::mdTokenSet        resolved;

    //
    // We iterate through all the TypeDefs, resolving dependencies among them.
    //
    // This is good, because the layout will be ordered and no multiple passes will be required on the runtime end.
    //
    for(it = m_pr->m_mapDef_Type.begin(); it != m_pr->m_mapDef_Type.end(); it++)
    {
        TINYCLR_CHECK_HRESULT(ResolveTypeDef( it->second, order, resolved ));
    }


    //
    // Now we walk through the 'order' list, no forward dependencies left.
    //

    //
    // First, create all the records.
    //
    for(itOrder = order.begin(); itOrder != order.end(); itOrder++)
    {
        MetaData::TypeDef& td = m_pr->m_mapDef_Type.find( *itOrder )->second;

        m_lookupIDs[td.m_td] = CLR_TkFromType( TBL_TypeDef, (CLR_UINT32)m_tableTypeDef.GetPos() );

        CLR_RECORD_TYPEDEF* dst = m_tableTypeDef.Alloc( 1 ); if(dst == NULL) REPORT_NO_MEMORY();
    }

    //
    // Then, populate them.
    //
    for(itOrder = order.begin(); itOrder != order.end(); itOrder++)
    {
        TINYCLR_CHECK_HRESULT(ProcessTypeDef( (mdTypeDef)*itOrder ));
    }

    TINYCLR_NOCLEANUP();
}



//--//

HRESULT WatchAssemblyBuilder::Linker::ProcessTypeDef( mdTypeDef tdIdx )
{
    struct BuiltinMapping
    {
        LPCWSTR   szName;
        CLR_UINT8 dataType;
    };

    static const BuiltinMapping c_Builtins[] =
    {
        { L"System.Boolean"                      , DATATYPE_BOOLEAN    },
        { L"System.Char"                         , DATATYPE_CHAR       },
        { L"System.SByte"                        , DATATYPE_I1         },
        { L"System.Byte"                         , DATATYPE_U1         },
        { L"System.Int16"                        , DATATYPE_I2         },
        { L"System.UInt16"                       , DATATYPE_U2         },
        { L"System.Int32"                        , DATATYPE_I4         },
        { L"System.UInt32"                       , DATATYPE_U4         },
        { L"System.Int64"                        , DATATYPE_I8         },
        { L"System.UInt64"                       , DATATYPE_U8         },
        { L"System.Single"                       , DATATYPE_R4         },
        { L"System.Double"                       , DATATYPE_R8         },

        { L"System.DateTime"                     , DATATYPE_DATETIME   },
        { L"System.TimeSpan"                     , DATATYPE_TIMESPAN   },
        { L"System.String"                       , DATATYPE_STRING     },

//      { L"System.Reflection.Assembly"          , DATATYPE_REFLECTION },
        { L"System.RuntimeTypeHandle"            , DATATYPE_REFLECTION },
        { L"System.RuntimeFieldHandle"           , DATATYPE_REFLECTION },
        { L"System.RuntimeMethodHandle"          , DATATYPE_REFLECTION },

        { L"System.WeakReference"                , DATATYPE_WEAKCLASS  },
        { L"Microsoft.SPOT.ExtendedWeakReference", DATATYPE_WEAKCLASS  },
    };


    TINYCLR_HEADER();

    MetaData::TypeDef&                td = m_pr->m_mapDef_Type.find( tdIdx )->second;
    MetaData::mdFieldDefListIter      itField;
    MetaData::mdMethodDefListIter     itMethod;


    CLR_RECORD_TYPEDEF* dst = m_tableTypeDef.GetRecordAt( CLR_DataFromTk( m_lookupIDs[td.m_td] ) );

    if(!AllocString( td.m_name, dst->name, dst->nameSpace )) REPORT_NO_MEMORY();

    dst->extends       = EncodeTypeRefOrDef( td.m_extends        );
    dst->enclosingType = EncodeToken       ( td.m_enclosingClass );

    switch(td.m_flags & tdVisibilityMask)
    {
    case tdNotPublic        : dst->flags = CLR_RECORD_TYPEDEF::TD_Scope_NotPublic        ; break;
    case tdPublic           : dst->flags = CLR_RECORD_TYPEDEF::TD_Scope_Public           ; break;
    case tdNestedPublic     : dst->flags = CLR_RECORD_TYPEDEF::TD_Scope_NestedPublic     ; break;
    case tdNestedPrivate    : dst->flags = CLR_RECORD_TYPEDEF::TD_Scope_NestedPrivate    ; break;
    case tdNestedFamily     : dst->flags = CLR_RECORD_TYPEDEF::TD_Scope_NestedFamily     ; break;
    case tdNestedAssembly   : dst->flags = CLR_RECORD_TYPEDEF::TD_Scope_NestedAssembly   ; break;
    case tdNestedFamANDAssem: dst->flags = CLR_RECORD_TYPEDEF::TD_Scope_NestedFamANDAssem; break;
    case tdNestedFamORAssem : dst->flags = CLR_RECORD_TYPEDEF::TD_Scope_NestedFamORAssem ; break;
    default                 : TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    switch(td.m_flags & tdClassSemanticsMask)
    {
    case tdInterface:
        dst->flags &= ~CLR_RECORD_TYPEDEF::TD_Semantics_Mask;
        dst->flags |=  CLR_RECORD_TYPEDEF::TD_Semantics_Interface;
        break;
    }

    if(td.m_flags & tdSerializable   ) dst->flags |= CLR_RECORD_TYPEDEF::TD_Serializable;
    if(td.m_flags & tdAbstract       ) dst->flags |= CLR_RECORD_TYPEDEF::TD_Abstract;
    if(td.m_flags & tdSealed         ) dst->flags |= CLR_RECORD_TYPEDEF::TD_Sealed;
    if(td.m_flags & tdSpecialName    ) dst->flags |= CLR_RECORD_TYPEDEF::TD_SpecialName;
    if(td.m_flags & tdBeforeFieldInit) dst->flags |= CLR_RECORD_TYPEDEF::TD_BeforeFieldInit;
    if(td.m_flags & tdHasSecurity    ) dst->flags |= CLR_RECORD_TYPEDEF::TD_HasSecurity;

    if(MetaData::IsTokenPresent( m_setAttributes_Types, td.m_td ))
    {
        dst->flags |= CLR_RECORD_TYPEDEF::TD_HasAttributes;
    }

    if(IsNilToken(td.m_extends) == false)
    {
        std::wstring* name;

        if(TypeFromToken( td.m_extends ) == mdtTypeDef)
        {
            name = &m_pr->m_mapDef_Type.find(td.m_extends)->second.m_name;
        }
        else
        {
            name = &m_pr->m_mapRef_Type[td.m_extends].m_name;
        }

        if(td.m_flags & tdSealed)
        {
            if(*name == L"System.ValueType")
            {
                dst->flags &= ~CLR_RECORD_TYPEDEF::TD_Semantics_Mask;
                dst->flags |=  CLR_RECORD_TYPEDEF::TD_Semantics_ValueType;
            }

            if(*name == L"System.Enum")
            {
                dst->flags &= ~CLR_RECORD_TYPEDEF::TD_Semantics_Mask;
                dst->flags |=  CLR_RECORD_TYPEDEF::TD_Semantics_Enum;
                dst->flags |=  CLR_RECORD_TYPEDEF::TD_Serializable;
            }
        }

        if(*name == L"System.Delegate" && td.m_name != L"System.MulticastDelegate")
        {
            dst->flags |= CLR_RECORD_TYPEDEF::TD_Delegate;
        }

        if(*name == L"System.MulticastDelegate")
        {
            dst->flags |= CLR_RECORD_TYPEDEF::TD_MulticastDelegate;
        }
    }


    //
    // Built-in types detection
    //
    {
        LPCWSTR szName = td.m_name.c_str();
        int n = 0;

        for(n=0; n<ARRAYSIZE(c_Builtins); n++)
        {
            if(!wcscmp( c_Builtins[n].szName, szName )) break;
        }

        if(n < ARRAYSIZE(c_Builtins))
        {
            dst->dataType = c_Builtins[n].dataType;
        }
        else
        {
            switch(dst->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask)
            {
            case CLR_RECORD_TYPEDEF::TD_Semantics_ValueType:
                dst->dataType = DATATYPE_VALUETYPE;
                break;

            case CLR_RECORD_TYPEDEF::TD_Semantics_Enum:
                {
                    dst->dataType = DATATYPE_I4;

                    for(MetaData::mdFieldDefListIter itFieldDef = td.m_fields.begin(); itFieldDef != td.m_fields.end(); itFieldDef++)
                    {
                        MetaData::FieldDef& fd = m_pr->m_mapDef_Field.find( *itFieldDef )->second;

                        if(fd.m_flags & fdLiteral) continue;

                        if(fd.m_flags & fdSpecialName)
                        {
                            dst->dataType = MapElementTypeToDataType( fd.m_sig.m_sigField.m_opt );
                            break;
                        }
                    }
                }
                break;

            case CLR_RECORD_TYPEDEF::TD_Semantics_Class:
            case CLR_RECORD_TYPEDEF::TD_Semantics_Interface:
                dst->dataType = DATATYPE_CLASS;
                break;
            }
        }
    }

    //--//

    //
    // Static Fields.
    //
    dst->sFields_First = (CLR_IDX)m_tableFieldDef.GetPos();
    for(itField = td.m_fields.begin(); itField != td.m_fields.end(); itField++)
    {
        MetaData::FieldDefMapIter itFM = m_pr->m_mapDef_Field.find( *itField );

        if(itFM == m_pr->m_mapDef_Field.end())
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        MetaData::FieldDef& fd = itFM->second;

        TINYCLR_CHECK_HRESULT(ProcessFieldDef( td, dst, fd, fdStatic ));
    }
    TINYCLR_CHECK_HRESULT(m_tableFieldDef.VerifyRange( dst->sFields_First, 0x00FF, L"number", L"static fields" ));
    dst->sFields_Num = (CLR_UINT8)((CLR_IDX)m_tableFieldDef.GetPos() - dst->sFields_First);


    //
    // Instance Fields.
    //
    dst->iFields_First = (CLR_UINT32)m_tableFieldDef.GetPos();
    for(itField = td.m_fields.begin(); itField != td.m_fields.end(); itField++)
    {
        MetaData::FieldDefMapIter itFM = m_pr->m_mapDef_Field.find( *itField );

        if(itFM == m_pr->m_mapDef_Field.end())
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        MetaData::FieldDef& fd = itFM->second;

        TINYCLR_CHECK_HRESULT(ProcessFieldDef( td, dst, fd, 0 ));
    }
    TINYCLR_CHECK_HRESULT(m_tableFieldDef.VerifyRange( dst->iFields_First, 0xFF, L"number", L"instance fields" ));
    dst->iFields_Num = (CLR_UINT8)((CLR_IDX)m_tableFieldDef.GetPos() - dst->iFields_First);

    TINYCLR_CHECK_HRESULT(m_tableFieldDef.VerifyRange( 0, 0xFFFF, L"number", L"fields" ));

    //--//

    SIZE_T lastMethod = m_tableMethodDef.GetPos();

    dst->methods_First = (CLR_IDX)lastMethod;

    //
    // Virtual Methods.
    //
    for(itMethod = td.m_methods.begin(); itMethod != td.m_methods.end(); itMethod++)
    {
        MetaData::MethodDefMapIter itDM = m_pr->m_mapDef_Method.find( *itMethod );

        if(itDM == m_pr->m_mapDef_Method.end())
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        MetaData::MethodDef& md = itDM->second;

        TINYCLR_CHECK_HRESULT(ProcessMethodDef( td, dst, md, mdVirtual ));
    }
    TINYCLR_CHECK_HRESULT(m_tableMethodDef.VerifyRange( lastMethod, 0xFF, L"number", L"virtual methods" ));
    dst->vMethods_Num = (CLR_UINT8)((CLR_IDX)m_tableMethodDef.GetPos() - lastMethod); lastMethod += dst->vMethods_Num;



    //
    // Instance Methods.
    //
    for(itMethod = td.m_methods.begin(); itMethod != td.m_methods.end(); itMethod++)
    {
        MetaData::MethodDefMapIter itDM = m_pr->m_mapDef_Method.find( *itMethod );

        if(itDM == m_pr->m_mapDef_Method.end())
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        MetaData::MethodDef& md = itDM->second;

        TINYCLR_CHECK_HRESULT(ProcessMethodDef( td, dst, md, 0 ));
    }
    TINYCLR_CHECK_HRESULT(m_tableMethodDef.VerifyRange( lastMethod, 0xFF, L"number", L"instance methods" ));
    dst->iMethods_Num = (CLR_UINT8)((CLR_IDX)m_tableMethodDef.GetPos() - lastMethod); lastMethod += dst->iMethods_Num;


    //
    // Static Methods.
    //
    for(itMethod = td.m_methods.begin(); itMethod != td.m_methods.end(); itMethod++)
    {
        MetaData::MethodDefMapIter itDM = m_pr->m_mapDef_Method.find( *itMethod );

        if(itDM == m_pr->m_mapDef_Method.end())
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        MetaData::MethodDef& md = itDM->second;

        TINYCLR_CHECK_HRESULT(ProcessMethodDef( td, dst, md, mdStatic ));
    }
    TINYCLR_CHECK_HRESULT(m_tableMethodDef.VerifyRange( lastMethod, 0xFF, L"number", L"static methods" ));
    dst->sMethods_Num = (CLR_UINT8)((CLR_IDX)m_tableMethodDef.GetPos() - lastMethod); lastMethod += dst->iMethods_Num;

    //--//

    TINYCLR_CHECK_HRESULT(CheckRange( td.m_interfaces, (CLR_UINT8)0, L"number", L"interfaces" ));

    if(!AllocSignatureForInterfaces( td.m_interfaces, dst->interfaces )) REPORT_NO_MEMORY();

    //--//

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        wprintf( L"Failed compiling type '%s'\n", td.m_name.c_str() );
    }

    TINYCLR_CLEANUP_END();
}

//--//

HRESULT WatchAssemblyBuilder::Linker::ProcessTypeDef_ByteCode( mdTypeDef tdIdx )
{
    TINYCLR_HEADER();

    MetaData::TypeDef&                td = m_pr->m_mapDef_Type.find( tdIdx )->second;
    MetaData::mdMethodDefListIter     itMethod;

    CLR_RECORD_TYPEDEF* dst = m_tableTypeDef.GetRecordAt( CLR_DataFromTk( m_lookupIDs[td.m_td] ) );

    //--//
    if ( m_fBigEndianTarget )
    {
        for(itMethod = td.m_methods.begin(); itMethod != td.m_methods.end(); itMethod++)
        {
            MetaData::MethodDefMapIter itDM = m_pr->m_mapDef_Method.find( *itMethod );

            if(itDM == m_pr->m_mapDef_Method.end())
            {
                TINYCLR_SET_AND_LEAVE(E_FAIL);
            }

            MetaData::MethodDef& md = itDM->second;

            size_t             len    = md.m_byteCode.m_opcodes.size();
            for(size_t i=0; i<len; i++)
            {
                MetaData::ByteCode::LogicalOpcodeDesc& opCode = md.m_byteCode.m_opcodes[i];

                // Swap the argument data for each of CEE_LDC_I4/R4/I8/R8
                switch (opCode.m_op)
                {
                case CEE_LDC_I4:
                    opCode.m_arg_I4 = (CLR_INT32)SwapEndian( (UINT32)(opCode.m_arg_I4) );
                    break;
                case CEE_LDC_R4:
                    opCode.m_arg_R4 = (CLR_INT32)SwapEndian( (UINT32)opCode.m_arg_R4 );
                    break;
                case CEE_LDC_I8:
                    opCode.m_arg_I8 = (CLR_INT64)SwapEndian( (UINT64)opCode.m_arg_I8 );
                    break;
                case CEE_LDC_R8:
                    opCode.m_arg_R8 = (CLR_INT64)SwapEndian( (UINT64)opCode.m_arg_R8 );
                    break;
                default: 
                    break;
                }
            }
        }
    }
    //
    // Virtual Methods.
    //
    for(itMethod = td.m_methods.begin(); itMethod != td.m_methods.end(); itMethod++)
    {
        MetaData::MethodDefMapIter itDM = m_pr->m_mapDef_Method.find( *itMethod );

        if(itDM == m_pr->m_mapDef_Method.end())
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        MetaData::MethodDef& md = itDM->second;

        TINYCLR_CHECK_HRESULT(ProcessMethodDef_ByteCode( td, dst, md, mdVirtual ));
    }

    //
    // Instance Methods.
    //
    for(itMethod = td.m_methods.begin(); itMethod != td.m_methods.end(); itMethod++)
    {
        MetaData::MethodDefMapIter itDM = m_pr->m_mapDef_Method.find( *itMethod );

        if(itDM == m_pr->m_mapDef_Method.end())
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        MetaData::MethodDef& md = itDM->second;

        TINYCLR_CHECK_HRESULT(ProcessMethodDef_ByteCode( td, dst, md, 0 ));
    }

    //
    // Static Methods.
    //
    for(itMethod = td.m_methods.begin(); itMethod != td.m_methods.end(); itMethod++)
    {
        MetaData::MethodDefMapIter itDM = m_pr->m_mapDef_Method.find( *itMethod );

        if(itDM == m_pr->m_mapDef_Method.end())
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        MetaData::MethodDef& md = itDM->second;

        TINYCLR_CHECK_HRESULT(ProcessMethodDef_ByteCode( td, dst, md, mdStatic ));
    }

    //--//

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        wprintf( L"Failed compiling type '%s'\n", td.m_name.c_str() );
    }

    TINYCLR_CLEANUP_END();
}

//--//

HRESULT WatchAssemblyBuilder::Linker::ProcessFieldDef( MetaData::TypeDef& td, CLR_RECORD_TYPEDEF* tdDst, MetaData::FieldDef& fd, CLR_UINT32 mode )
{
    TINYCLR_HEADER();

    if(fd.m_flags & fdLiteral)
    {
        TINYCLR_SET_AND_LEAVE(S_FALSE); // Don't generate constant fields.
    }

    if(fd.m_flags & fdStatic)
    {
        if(mode != fdStatic)
        {
            TINYCLR_SET_AND_LEAVE(S_FALSE);
        }
    }
    else
    {
        if(mode != 0)
        {
            TINYCLR_SET_AND_LEAVE(S_FALSE);
        }
    }

    //--//

    m_lookupIDs[fd.m_fd] = CLR_TkFromType( TBL_FieldDef, (CLR_UINT32)m_tableFieldDef.GetPos() );

    CLR_RECORD_FIELDDEF* dst = m_tableFieldDef.Alloc( 1 ); if(dst == NULL) REPORT_NO_MEMORY();


    if(!AllocString           (  fd.m_name          , dst->name, false )) REPORT_NO_MEMORY();
    if(!AllocSignatureForField( &fd.m_sig.m_sigField, dst->sig         )) REPORT_NO_MEMORY();

    switch(fd.m_flags & fdFieldAccessMask)
    {
    case fdPrivateScope: dst->flags = CLR_RECORD_FIELDDEF::FD_Scope_PrivateScope; break;
    case fdPrivate     : dst->flags = CLR_RECORD_FIELDDEF::FD_Scope_Private     ; break;
    case fdFamANDAssem : dst->flags = CLR_RECORD_FIELDDEF::FD_Scope_FamANDAssem ; break;
    case fdAssembly    : dst->flags = CLR_RECORD_FIELDDEF::FD_Scope_Assembly    ; break;
    case fdFamily      : dst->flags = CLR_RECORD_FIELDDEF::FD_Scope_Family      ; break;
    case fdFamORAssem  : dst->flags = CLR_RECORD_FIELDDEF::FD_Scope_FamORAssem  ; break;
    case fdPublic      : dst->flags = CLR_RECORD_FIELDDEF::FD_Scope_Public      ; break;
    default            : TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    if(fd.m_flags & fdStatic           ) dst->flags |= CLR_RECORD_FIELDDEF::FD_Static       ;
    if(fd.m_flags & fdInitOnly         ) dst->flags |= CLR_RECORD_FIELDDEF::FD_InitOnly     ;
    if(fd.m_flags & fdLiteral          ) dst->flags |= CLR_RECORD_FIELDDEF::FD_Literal      ;
    if(fd.m_flags & fdNotSerialized    ) dst->flags |= CLR_RECORD_FIELDDEF::FD_NotSerialized;

    if(fd.m_flags & fdSpecialName      ) dst->flags |= CLR_RECORD_FIELDDEF::FD_SpecialName;
    if(fd.m_flags & fdHasDefault       ) dst->flags |= CLR_RECORD_FIELDDEF::FD_HasDefault ;
    if(fd.m_flags & fdHasFieldRVA      ) dst->flags |= CLR_RECORD_FIELDDEF::FD_HasFieldRVA;


    if(fd.m_value.size() > 0)
    {
        CLR_RT_Buffer valueWithSize;
        CLR_UINT16    len = (CLR_UINT16)fd.m_value.size();

        valueWithSize.resize( len + sizeof(len) );

        memcpy( &valueWithSize[0            ], &len          , sizeof(len) );
        memcpy( &valueWithSize[0+sizeof(len)], &fd.m_value[0],        len  );

        if(!FlushSignature( dst->defaultValue, &valueWithSize[0], (int)valueWithSize.size() )) REPORT_NO_MEMORY();
    }
    else
    {
        dst->defaultValue = CLR_EmptyIndex;
    }

    if(MetaData::IsTokenPresent( m_setAttributes_Fields, fd.m_fd ))
    {
        dst->flags |= CLR_RECORD_FIELDDEF::FD_HasAttributes;
    }

    if(MetaData::IsTokenPresent( m_pr->m_setAttributes_Fields_NoReflection, fd.m_fd ))
    {
        dst->flags |= CLR_RECORD_FIELDDEF::FD_NoReflection;
    }

    //--//

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        wprintf( L"Failed compiling field '%s.%s'\n", td.m_name.c_str(), fd.m_name.c_str() );
    }

    TINYCLR_CLEANUP_END();
}

//--//

HRESULT WatchAssemblyBuilder::Linker::ProcessMethodDef( MetaData::TypeDef& td, CLR_RECORD_TYPEDEF* tdDst, MetaData::MethodDef& md, CLR_UINT32 mode )
{
    TINYCLR_HEADER();

    LPCWSTR szName = md.m_name.c_str();

    if(md.m_flags & mdStatic)
    {
        if(mode != mdStatic)
        {
            TINYCLR_SET_AND_LEAVE(S_FALSE);
        }
    }
    else if(md.m_flags & mdVirtual)
    {
        if(mode != mdVirtual)
        {
            TINYCLR_SET_AND_LEAVE(S_FALSE);
        }
    }
    else
    {
        if(mode != 0)
        {
            TINYCLR_SET_AND_LEAVE(S_FALSE);
        }
    }

    //--//

    m_lookupIDs[md.m_md] = CLR_TkFromType( TBL_MethodDef, (CLR_UINT32)m_tableMethodDef.GetPos() );

    CLR_RECORD_METHODDEF* dst = m_tableMethodDef.Alloc( 1 ); if(dst == NULL) REPORT_NO_MEMORY();

    switch(md.m_flags & mdMemberAccessMask)
    {
    case mdPrivateScope: dst->flags = CLR_RECORD_METHODDEF::MD_Scope_PrivateScope; break;
    case mdPrivate     : dst->flags = CLR_RECORD_METHODDEF::MD_Scope_Private     ; break;
    case mdFamANDAssem : dst->flags = CLR_RECORD_METHODDEF::MD_Scope_FamANDAssem ; break;
    case mdAssem       : dst->flags = CLR_RECORD_METHODDEF::MD_Scope_Assem       ; break;
    case mdFamily      : dst->flags = CLR_RECORD_METHODDEF::MD_Scope_Family      ; break;
    case mdFamORAssem  : dst->flags = CLR_RECORD_METHODDEF::MD_Scope_FamORAssem  ; break;
    case mdPublic      : dst->flags = CLR_RECORD_METHODDEF::MD_Scope_Public      ; break;
    default            : TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    if(md.m_flags & mdStatic          ) dst->flags |= CLR_RECORD_METHODDEF::MD_Static          ;
    if(md.m_flags & mdFinal           ) dst->flags |= CLR_RECORD_METHODDEF::MD_Final           ;
    if(md.m_flags & mdVirtual         ) dst->flags |= CLR_RECORD_METHODDEF::MD_Virtual         ;
    if(md.m_flags & mdHideBySig       ) dst->flags |= CLR_RECORD_METHODDEF::MD_HideBySig       ;

    if(md.m_flags & mdNewSlot         ) dst->flags |= CLR_RECORD_METHODDEF::MD_NewSlot         ;

    if(md.m_flags & mdAbstract        ) dst->flags |= CLR_RECORD_METHODDEF::MD_Abstract        ;
    if(md.m_flags & mdSpecialName     ) dst->flags |= CLR_RECORD_METHODDEF::MD_SpecialName     ;

    if(md.m_flags & mdHasSecurity     ) dst->flags |= CLR_RECORD_METHODDEF::MD_HasSecurity     ;
    if(md.m_flags & mdRequireSecObject) dst->flags |= CLR_RECORD_METHODDEF::MD_RequireSecObject;


    if(md.m_implFlags & miRuntime)
    {
        //
        // It should be MulticastDelegate or Delegate
        //
        if((tdDst->flags & (CLR_RECORD_TYPEDEF::TD_Delegate | CLR_RECORD_TYPEDEF::TD_MulticastDelegate)) == 0)
        {
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        if(!wcscmp( szName, L".ctor"       )) dst->flags |= CLR_RECORD_METHODDEF::MD_DelegateConstructor;
        if(!wcscmp( szName, L"Invoke"      )) dst->flags |= CLR_RECORD_METHODDEF::MD_DelegateInvoke;
        if(!wcscmp( szName, L"BeginInvoke" )) dst->flags |= CLR_RECORD_METHODDEF::MD_DelegateBeginInvoke;
        if(!wcscmp( szName, L"EndInvoke"   )) dst->flags |= CLR_RECORD_METHODDEF::MD_DelegateEndInvoke;
    }


    if(IsMdInstanceInitializerW( md.m_flags, szName )) dst->flags |= CLR_RECORD_METHODDEF::MD_Constructor;
    if(IsMdClassConstructorW   ( md.m_flags, szName )) dst->flags |= CLR_RECORD_METHODDEF::MD_StaticConstructor;

    if(!wcscmp( szName, L"Finalize" ) && md.m_method.m_retValue.m_opt == ELEMENT_TYPE_VOID && md.m_method.m_lstParams.size() == 0)
    {
        dst->flags |= CLR_RECORD_METHODDEF::MD_Finalizer;

        if(td.m_name != L"System.Object")
        {
            tdDst->flags |= CLR_RECORD_TYPEDEF::TD_HasFinalizer;
        }
    }

    if(md.m_implFlags & miSynchronized)
    {
        dst->flags |= CLR_RECORD_METHODDEF::MD_Synchronized;
    }

    if(MetaData::IsTokenPresent( m_pr->m_setAttributes_Methods_NativeProfiler, md.m_md ))
    {
        dst->flags |= CLR_RECORD_METHODDEF::MD_NativeProfiled;
    }

    if(MetaData::IsTokenPresent( m_pr->m_setAttributes_Methods_GloballySynchronized, md.m_md ))
    {
        dst->flags |= CLR_RECORD_METHODDEF::MD_GloballySynchronized;
    }

    if(MetaData::IsTokenPresent( m_setAttributes_Methods, md.m_md ))
    {
        dst->flags |= CLR_RECORD_METHODDEF::MD_HasAttributes;
    }

    if(m_pr->m_entryPointToken == md.m_md)
    {
        dst->flags |= CLR_RECORD_METHODDEF::MD_EntryPoint;
    }

    if(!AllocString            (  md.m_name  , dst->name  , false )) REPORT_NO_MEMORY();
    if(!AllocSignatureForMethod( &md.m_method, dst->sig           )) REPORT_NO_MEMORY();

    TINYCLR_CHECK_HRESULT(CheckRange( md.m_method.m_lstParams, dst->numArgs, L"number", L"arguments" ));

    dst->numArgs = (CLR_UINT8)                          md.m_method.m_lstParams.size()  ; if((md.m_flags & mdStatic) == 0) dst->numArgs++; // Include the 'this' pointer in the number of arguments.
    dst->retVal  = (CLR_UINT8)MapElementTypeToDataType( md.m_method.m_retValue .m_opt  );

    if(m_pr->m_fNoByteCode || (md.m_flags & mdAbstract) || (md.m_implFlags & (miRuntime | miInternalCall)))
    {
        dst->numLocals = 0;
    }
    else
    {
        TINYCLR_CHECK_HRESULT(CheckRange( md.m_vars.m_lstVars, dst->numLocals, L"number", L"locals" ));

        dst->numLocals = (CLR_UINT8)md.m_vars.m_lstVars.size();

        if(!AllocSignatureForLocalVar(  md.m_vars, dst->locals )) REPORT_NO_MEMORY();
    }

    //--//

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        wprintf( L"Failed compiling method '%s.%s'\n", td.m_name.c_str(), md.m_name.c_str() );
    }

    TINYCLR_CLEANUP_END();
}

//--//

HRESULT WatchAssemblyBuilder::Linker::ProcessMethodDef_ByteCode( MetaData::TypeDef& td, CLR_RECORD_TYPEDEF* tdDst, MetaData::MethodDef& md, CLR_UINT32 mode )
{
    TINYCLR_HEADER();

    LPCWSTR szName = md.m_name.c_str();

    if(md.m_flags & mdStatic)
    {
        if(mode != mdStatic)
        {
            TINYCLR_SET_AND_LEAVE(S_FALSE);
        }
    }
    else if(md.m_flags & mdVirtual)
    {
        if(mode != mdVirtual)
        {
            TINYCLR_SET_AND_LEAVE(S_FALSE);
        }
    }
    else
    {
        if(mode != 0)
        {
            TINYCLR_SET_AND_LEAVE(S_FALSE);
        }
    }

    //--//

    CLR_RECORD_METHODDEF* dst = m_tableMethodDef.GetRecordAt( CLR_DataFromTk( m_lookupIDs[md.m_md] ) );

    if(m_pr->m_fNoByteCode || (md.m_flags & mdAbstract) || (md.m_implFlags & (miRuntime | miInternalCall)))
    {
        dst->RVA = CLR_EmptyIndex;
    }
    else if(md.m_byteCode.m_opcodes.size() > 0)
    {
        std::vector<BYTE> code;
        CLR_UINT32        stackDepth;

        TINYCLR_CHECK_HRESULT(md.m_byteCode.ConvertTokens( m_lookupIDs      ));
        TINYCLR_CHECK_HRESULT(md.m_byteCode.GenerateOldIL( code, m_fBigEndianTarget ));

        //--//

        stackDepth = md.m_byteCode.MaxStackDepth();

        if(stackDepth > md.m_maxStack)
        {
            //We have computed we need more stack space than the IL metadata claims.
            //It's ok if we need less (apparently, sometimes the c# compiler appears to ask for more than necessary)
            wprintf( L"Bad stack depth" );
            TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        }

        TINYCLR_CHECK_HRESULT(CheckRange( stackDepth, dst->lengthEvalStack, L"depth", L"stack" ));

        dst->lengthEvalStack = (CLR_UINT8)stackDepth;

        //--//

        const BYTE* byteCodeSrc   = &code[0];
        size_t      byteCodeLen   =  code.size();
        size_t      numExceptions =  md.m_byteCode.m_exceptions.size();

        //--//

        dst->RVA = (CLR_OFFSET)m_tableByteCode.Size();

        BYTE* byteCodeDst = m_tableByteCode.Alloc( byteCodeLen ); if(byteCodeDst == NULL) REPORT_NO_MEMORY();

        memcpy( byteCodeDst, byteCodeSrc, byteCodeLen );

        //--//

        if(numExceptions > 0)
        {
            ExceptionHandlerHierarchy lookupEh;

            dst->flags |= CLR_RECORD_METHODDEF::MD_HasExceptionHandlers;

            for(size_t i=0; i<numExceptions; i++)
            {
                MetaData::ByteCode::LogicalExceptionBlock& leb = md.m_byteCode.m_exceptions[i];

                CLR_RECORD_EH eh; TINYCLR_CLEAR(eh);

                switch(leb.m_Flags)
                {
                case COR_ILEXCEPTION_CLAUSE_FINALLY:
                    eh.mode = CLR_RECORD_EH::EH_Finally;
                    break;

                case COR_ILEXCEPTION_CLAUSE_FILTER:
                    eh.mode = CLR_RECORD_EH::EH_Filter;
                    eh.filterStart     = (CLR_OFFSET)(leb.m_FilterOffset);
                    break;

                case COR_ILEXCEPTION_CLAUSE_FAULT:
                case COR_ILEXCEPTION_CLAUSE_NONE:
                    if(IsNilToken(leb.m_ClassToken))
                    {
                        eh.mode = CLR_RECORD_EH::EH_CatchAll;
                    }
                    else
                    {
                        eh.mode       = CLR_RECORD_EH::EH_Catch;
                        eh.classToken = EncodeTypeRefOrDef( leb.m_ClassToken );
                    }
                    break;

                default:
                    wprintf( L"Bad exception handler" );
                    TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
                    break;
                }

                eh.tryStart     = (CLR_OFFSET)(leb.m_TryOffset                          );
                eh.tryEnd       = (CLR_OFFSET)(leb.m_TryOffset     + leb.m_TryLength    );
                eh.handlerStart = (CLR_OFFSET)(leb.m_HandlerOffset                      );
                eh.handlerEnd   = (CLR_OFFSET)(leb.m_HandlerOffset + leb.m_HandlerLength);

                lookupEh.Queue( eh );
            }

            {
                CQuickRecord<CLR_RECORD_EH> tableEh;
                CLR_OFFSET_LONG             start;

                TINYCLR_CHECK_HRESULT(lookupEh.GenerateOutput( tableEh ));

                TINYCLR_CHECK_HRESULT(tableEh.CopyTo( m_tableByteCode, start ));
            }

            byteCodeDst = m_tableByteCode.Alloc( 1 ); if(byteCodeDst == NULL) REPORT_NO_MEMORY();

            *byteCodeDst = (BYTE)numExceptions;
        }
    }

    //--//

    TINYCLR_CLEANUP();

    if(FAILED(hr))
    {
        wprintf( L"Failed compiling method '%s.%s'\n", td.m_name.c_str(), md.m_name.c_str() );
    }

    TINYCLR_CLEANUP_END();
}

//--//

WatchAssemblyBuilder::Linker::CustomAttributeId::CustomAttributeId( Linker* lk, MetaData::CustomAttribute& ca ) : m_ca(ca)
{
    switch(TypeFromToken( ca.m_tkObj ))
    {
    case mdtTypeDef  : m_id.ownerType = TBL_TypeDef  ; break;
    case mdtFieldDef : m_id.ownerType = TBL_FieldDef ; break;
    case mdtMethodDef: m_id.ownerType = TBL_MethodDef; break;
    }

    m_id.ownerIdx    = lk->m_lookupIDs[ ca.m_tkObj  ];
    m_id.constructor = lk->m_lookupIDs[ ca.m_tkType ];

    switch(TypeFromToken( ca.m_tkType ))
    {
    case mdtMemberRef: m_id.constructor |= 0x8000; break;
    case mdtMethodDef:                             break;
    }

    m_id.data = 0;
}

bool WatchAssemblyBuilder::Linker::CustomAttributeId::operator<( const CustomAttributeId& r ) const
{
    CLR_UINT32 left;
    CLR_UINT32 right;

    left  =   m_id.ownerType;
    right = r.m_id.ownerType;
    if(left == right)
    {
        left  =   m_id.ownerIdx;
        right = r.m_id.ownerIdx;
        if(left == right)
        {
            left  =   m_id.constructor;
            right = r.m_id.constructor;
        }
    }

    return ((CLR_INT32)(left - right)) < 0;
}

//--//

HRESULT WatchAssemblyBuilder::Linker::ProcessAttribute()
{
    TINYCLR_HEADER();

    CustomAttributeIdSet set;

    for(MetaData::CustomAttributeMapIter itCA = m_pr->m_mapDef_CustomAttribute.begin(); itCA != m_pr->m_mapDef_CustomAttribute.end(); itCA++)
    {
        CustomAttributeId id( this, itCA->second );

        set.insert( id );
    }

    for(CustomAttributeIdSetIter itS = set.begin(); itS != set.end(); itS++)
    {
        CustomAttributeId id = *itS;

        if(!AllocSignatureForAttribute( id.m_ca, id.m_id.data )) REPORT_NO_MEMORY();

        CLR_RECORD_ATTRIBUTE* dst = m_tableAttribute.Alloc( 1 ); if(dst == NULL) REPORT_NO_MEMORY();

        *dst = id.m_id;
    }

    TINYCLR_NOCLEANUP();
}

//--//

static HRESULT AlignToWORD( WatchAssemblyBuilder::CQuickRecord<BYTE>& buf )
{
    TINYCLR_HEADER();

    CLR_UINT32 diff = (CLR_UINT32)(buf.Size() % sizeof(CLR_UINT32));

    if(diff)
    {
        if(buf.Alloc( sizeof(CLR_UINT32) - diff ) == NULL) REPORT_NO_MEMORY();
    }

    TINYCLR_NOCLEANUP();
}

HRESULT WatchAssemblyBuilder::Linker::ProcessResource()
{
    TINYCLR_HEADER();

    std::wstring                 str;
    CLR_RT_Buffer                buf;
    CLR_UINT8*                   ptr;
    CLR_UINT8*                   dataDst;
    CLR_UINT8*                   ptrResourceHeader;
    CLR_UINT8*                   ptrResourceData;
    CLR_UINT8*                   ptrMax;
    TinyResourcesFileHeader*     fileHeader;              
    TinyResourcesResourceHeader* resource;
    CLR_INT16                    idLast = 0;
    CLR_RECORD_RESOURCE*         resourceDst;
    CLR_RECORD_RESOURCE_FILE*    resourceFile;
    std::string                  strName;
    bool                         fNeeds32bitAlignment;

    for(CLR_RT_StringSet::iterator it = m_pr->m_resources.begin(); it != m_pr->m_resources.end(); it++)
    {                    
        str = *it;
        
        //Read file into buffer
        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( str.c_str(), buf ));

        ptr    = &buf[0];
        ptrMax = ptr + buf.size();
       
        fileHeader        = (TinyResourcesFileHeader*)ptr;
        ptr              += fileHeader->sizeOfHeader;        
        ptrResourceHeader = ptr;

        if(ptr > ptrMax) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        
        //Assert that we have a valid .tinyresources
        if(fileHeader->magicNumber != TinyResourcesFileHeader::MAGIC_NUMBER    ) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        if(fileHeader->version     != CLR_RECORD_RESOURCE_FILE::CURRENT_VERSION) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        if(fileHeader->sizeOfResourceHeader < sizeof(CLR_RECORD_RESOURCE)      ) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
        
        //write resource file header       
        resourceFile                       = m_tableResourceFile.Alloc( 1 ); FAULT_ON_NULL(resourceFile);
        if ( m_fBigEndianTarget )
        {
            resourceFile->version              = SwapEndian(CLR_RECORD_RESOURCE_FILE::CURRENT_VERSION);
            resourceFile->numberOfResources    = SwapEndian(fileHeader->numberOfResources);
            resourceFile->sizeOfHeader         = SwapEndian(sizeof(resourceFile));
            resourceFile->sizeOfResourceHeader = SwapEndian(fileHeader->sizeOfResourceHeader);
            resourceFile->pad                  = 0;
            resourceFile->offset               = SwapEndian((CLR_UINT32)m_tableResource.Size() / sizeof(CLR_RECORD_RESOURCE));
        }
        else
        {
            resourceFile->version              = (CLR_RECORD_RESOURCE_FILE::CURRENT_VERSION);
            resourceFile->numberOfResources    = (fileHeader->numberOfResources);
            resourceFile->sizeOfHeader         = (sizeof(resourceFile));
            resourceFile->sizeOfResourceHeader = (fileHeader->sizeOfResourceHeader);
            resourceFile->pad                  = 0;
            resourceFile->offset               = ((CLR_UINT32)m_tableResource.Size() / sizeof(CLR_RECORD_RESOURCE));
        }
        
        CLR_RT_UnicodeHelper::ConvertToUTF8( str, strName );

        std::string::size_type iSep = strName.find_last_of( "\\/:" );

        if(iSep >= 0)
        {
            //Get filename from path
            strName = strName.substr( iSep + 1, strName.length() - iSep );
        }

        if(!AllocString(strName, resourceFile->name, true)) TINYCLR_SET_AND_LEAVE( CLR_E_OUT_OF_MEMORY );

        if (m_fBigEndianTarget)
        {
            resourceFile->name=SwapEndian(resourceFile->name);
        }

        for(CLR_UINT32 iResource = 0; iResource < fileHeader->numberOfResources; iResource++)
        {
            resource = (TinyResourcesResourceHeader*)ptr;
                        
            ptr += fileHeader->sizeOfResourceHeader;                        
            if(ptr > ptrMax) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            ptrResourceData = ptr;            
            
            ptr += resource->size;            
            if(ptr > ptrMax) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

            if(iResource > 0)
            {
                //Validate sorting of resource headers
                if(resource->id <= idLast) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }

            idLast = resource->id;
                        
            switch(resource->kind)                        
            {
            case CLR_RECORD_RESOURCE::RESOURCE_Binary:
            case CLR_RECORD_RESOURCE::RESOURCE_String:
                fNeeds32bitAlignment = false;
                break;
            case CLR_RECORD_RESOURCE::RESOURCE_Bitmap:            
            case CLR_RECORD_RESOURCE::RESOURCE_Font:
                fNeeds32bitAlignment = true;
                break;
            default:
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }
             
            //write resource header
            resourceDst         = m_tableResource.Alloc(1); FAULT_ON_NULL(resourceDst);
            resourceDst->id     = resource->id;
            resourceDst->flags  = 0;
            resourceDst->kind   = resource->kind;
            resourceDst->offset = (CLR_UINT32)m_tableResourceData.Size();
                        
            if(fNeeds32bitAlignment)
            {
                size_t sizeOld = m_tableResourceData.Size();
                int cPad;

                AlignToWORD( m_tableResourceData );
                
                cPad = (int)(m_tableResourceData.Size() - sizeOld);

                resourceDst->offset += cPad;
                resourceDst->flags  |= (CLR_RECORD_RESOURCE::FLAGS_PaddingMask & cPad);
            }

            //write resource data
            dataDst = m_tableResourceData.Alloc( resource->size ); FAULT_ON_NULL(dataDst);
            memcpy( dataDst, ptrResourceData, resource->size );
        }
    }

    if(m_pr->m_resources.size() > 0)
    {
        //Dump sentinel at end.  This is necessary to accurately compute the size of the last resource
        //The MMP will pad tables to get to 32-bit boundaries
        resourceDst         = m_tableResource.Alloc(1); FAULT_ON_NULL(resourceDst);        
        resourceDst->id     = CLR_RECORD_RESOURCE::SENTINEL_ID;
        resourceDst->kind   = CLR_RECORD_RESOURCE::RESOURCE_Invalid;
        resourceDst->offset = (CLR_UINT32)m_tableResourceData.Size();
        resourceDst->flags  = 0;
    }
    
    TINYCLR_NOCLEANUP();
}

//--//

HRESULT WatchAssemblyBuilder::Linker::ProcessTypeSpec()
{
    TINYCLR_HEADER();

    for(MetaData::TypeSpecMapIter it = m_pr->m_mapSpec_Type.begin(); it != m_pr->m_mapSpec_Type.end(); it++)
    {
        mdToken             tk = it->first;
        MetaData::TypeSpec& ts = it->second;

        m_lookupIDs[tk] = CLR_TkFromType( TBL_TypeSpec, (CLR_UINT32)m_tableTypeSpec.GetPos() );

        CLR_RECORD_TYPESPEC* dst = m_tableTypeSpec.Alloc( 1 ); if(dst == NULL) REPORT_NO_MEMORY();

        if(!AllocSignatureForTypeSpec( &ts.m_sig, dst->sig )) REPORT_NO_MEMORY();
    }

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT WatchAssemblyBuilder::Linker::ProcessUserString()
{
    TINYCLR_HEADER();

    MetaData::UserStringMapIter it;
    CLR_STRING                  idx;

    for(it = m_pr->m_mapDef_String.begin(); it != m_pr->m_mapDef_String.end(); it++)
    {
        mdToken       tk = it->first;
        std::wstring& us = it->second;

        if(!AllocString( us, idx, true )) REPORT_NO_MEMORY();

        m_lookupIDs[tk] = CLR_TkFromType( TBL_Strings, idx );
    }

    TINYCLR_NOCLEANUP();
}
//--//--//--//

HRESULT WatchAssemblyBuilder::Linker::ResolveTypeRef( MetaData::TypeRef& tr, MetaData::mdTypeRefList& order, MetaData::mdTokenSet& resolved )
{
    TINYCLR_HEADER();

    //
    // Resolve scope.
    //
    if(IsNilToken( tr.m_scope ))
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL); // We need a scope...
    }
    else
    {
        TINYCLR_CHECK_HRESULT(m_pr->CheckTokenPresence( tr.m_scope ));

        if(TypeFromToken( tr.m_scope ) == mdtTypeRef)
        {
            if(MetaData::IsTokenPresent( resolved, tr.m_scope ) == false)
            {
                TINYCLR_CHECK_HRESULT(ResolveTypeRef( m_pr->m_mapRef_Type[tr.m_scope], order, resolved ));
            }
        }
    }

    order   .push_back( tr.m_tr );
    resolved.insert   ( tr.m_tr );

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT WatchAssemblyBuilder::Linker::ResolveTypeDef( MetaData::TypeDef& td, MetaData::mdTypeDefList& order, MetaData::mdTokenSet& resolved )
{
    TINYCLR_HEADER();

    
    // First we check if this type is already in list of resolved types.
    // If token already present - we just return. 
    if ( MetaData::IsTokenPresent( resolved, td.m_td ) == true )
    {
        TINYCLR_SET_AND_LEAVE( S_OK );
    }
    //
    // Resolve enclosing class.
    //
    if(IsNilToken(td.m_enclosingClass) == false) // Nested class.
    {
        TINYCLR_CHECK_HRESULT(m_pr->CheckTokenPresence( td.m_enclosingClass ));

        if(TypeFromToken( td.m_enclosingClass ) == mdtTypeDef)
        {
            if(MetaData::IsTokenPresent( resolved, td.m_enclosingClass ) == false)
            {
                TINYCLR_CHECK_HRESULT(ResolveTypeDef( m_pr->m_mapDef_Type.find(td.m_enclosingClass)->second, order, resolved ));
            }
        }
    }

    //
    // Resolve extending class.
    //
    if(IsNilToken(td.m_extends)) // Nested class.
    {
        if((td.m_flags & tdInterface) == 0) // Not an interface...
        {
            //
            // Ops, this should not happen, unless it's the special case for root of hierarchy...
            //
            if(td.m_name != L"System.Object")
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
            }
        }
    }
    else
    {
        TINYCLR_CHECK_HRESULT(m_pr->CheckTokenPresence( td.m_extends ));

        if(TypeFromToken( td.m_extends ) == mdtTypeDef)
        {
            if(MetaData::IsTokenPresent( resolved, td.m_extends ) == false)
            {
                TINYCLR_CHECK_HRESULT(ResolveTypeDef( m_pr->m_mapDef_Type.find(td.m_extends)->second, order, resolved ));
            }
        }
    }

    //
    // Resolve interfaces.
    //
    for(MetaData::mdInterfaceImplListIter it = td.m_interfaces.begin(); it != td.m_interfaces.end(); it++)
    {
        MetaData::InterfaceImpl& ii = m_pr->m_mapDef_Interface[*it];

        if(TypeFromToken( ii.m_itf ) == mdtTypeDef)
        {
            if(MetaData::IsTokenPresent( resolved, ii.m_itf ) == false)
            {
                TINYCLR_CHECK_HRESULT(ResolveTypeDef( m_pr->m_mapDef_Type.find(ii.m_itf)->second, order, resolved ));
            }
        }
    }


    order   .push_back( td.m_td );
    resolved.insert   ( td.m_td );

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

template<typename T> HRESULT AppendToAssembly( WatchAssemblyBuilder::CQuickRecord<T>& input, WatchAssemblyBuilder::CQuickRecord<BYTE>& output, CLR_RECORD_ASSEMBLY& header, CLR_TABLESENUM tbl, size_t maxSize, LPCWSTR streamName )
{        
    TINYCLR_HEADER();

    TINYCLR_CHECK_HRESULT(input.CopyTo( output, header.startOfTables[tbl], maxSize ));

    size_t sizeOld = output.Size();

    TINYCLR_CHECK_HRESULT(AlignToWORD( output ));

    header.paddingOfTables[tbl] = (CLR_UINT8)(output.Size() - sizeOld);

    TINYCLR_CLEANUP();

    if(hr == CLR_E_OUT_OF_RANGE)
    {
        wprintf( L"Exceeded maximum size of stream '%s': %d\n", streamName, input.Size() );
    }

    TINYCLR_CLEANUP_END();
}

HRESULT WatchAssemblyBuilder::Linker::EmitData( CQuickRecord<BYTE>& buf, CLR_RECORD_ASSEMBLY& headerSrc )
{
    TINYCLR_HEADER();

    CLR_RECORD_ASSEMBLY* header;    

    header = (CLR_RECORD_ASSEMBLY*)buf.Alloc( sizeof(*header) ); if(header == NULL) REPORT_NO_MEMORY();

    //--//

    TINYCLR_CHECK_HRESULT(AppendToAssembly( m_tableAssemblyRef  , buf, headerSrc, TBL_AssemblyRef   , CLR_MaxStreamSize_AssemblyRef   , L"AssemblyRef"    ));
    TINYCLR_CHECK_HRESULT(AppendToAssembly( m_tableTypeRef      , buf, headerSrc, TBL_TypeRef       , CLR_MaxStreamSize_TypeRef       , L"TypeRef"        ));
    TINYCLR_CHECK_HRESULT(AppendToAssembly( m_tableFieldRef     , buf, headerSrc, TBL_FieldRef      , CLR_MaxStreamSize_FieldRef      , L"FieldRef"       ));
    TINYCLR_CHECK_HRESULT(AppendToAssembly( m_tableMethodRef    , buf, headerSrc, TBL_MethodRef     , CLR_MaxStreamSize_MethodRef     , L"MethodRef"      ));
    TINYCLR_CHECK_HRESULT(AppendToAssembly( m_tableTypeDef      , buf, headerSrc, TBL_TypeDef       , CLR_MaxStreamSize_TypeDef       , L"TypeDef"        ));
    TINYCLR_CHECK_HRESULT(AppendToAssembly( m_tableFieldDef     , buf, headerSrc, TBL_FieldDef      , CLR_MaxStreamSize_FieldDef      , L"FieldDef"       ));
    TINYCLR_CHECK_HRESULT(AppendToAssembly( m_tableMethodDef    , buf, headerSrc, TBL_MethodDef     , CLR_MaxStreamSize_MethodDef     , L"MethodDef"      ));
    TINYCLR_CHECK_HRESULT(AppendToAssembly( m_tableAttribute    , buf, headerSrc, TBL_Attributes    , CLR_MaxStreamSize_Attributes    , L"Attributes"     ));
    TINYCLR_CHECK_HRESULT(AppendToAssembly( m_tableTypeSpec     , buf, headerSrc, TBL_TypeSpec      , CLR_MaxStreamSize_TypeSpec      , L"TypeSpec"       ));
    TINYCLR_CHECK_HRESULT(AppendToAssembly( m_tableResource     , buf, headerSrc, TBL_Resources     , CLR_MaxStreamSize_Resources     , L"Resources"      ));
    TINYCLR_CHECK_HRESULT(AppendToAssembly( m_tableResourceData , buf, headerSrc, TBL_ResourcesData , CLR_MaxStreamSize_ResourcesData , L"ResourcesData"  ));
    TINYCLR_CHECK_HRESULT(AppendToAssembly( m_tableString       , buf, headerSrc, TBL_Strings       , CLR_MaxStreamSize_Strings       , L"Strings"        ));
    TINYCLR_CHECK_HRESULT(AppendToAssembly( m_tableSignature    , buf, headerSrc, TBL_Signatures    , CLR_MaxStreamSize_Signatures    , L"Signatures"     ));
    TINYCLR_CHECK_HRESULT(AppendToAssembly( m_tableByteCode     , buf, headerSrc, TBL_ByteCode      , CLR_MaxStreamSize_ByteCode      , L"ByteCode"       ));
    TINYCLR_CHECK_HRESULT(AppendToAssembly( m_tableResourceFile , buf, headerSrc, TBL_ResourcesFiles, CLR_MaxStreamSize_ResourcesFiles, L"ResourcesFiles" ));

    headerSrc.startOfTables[TBL_EndOfAssembly]= (CLR_OFFSET_LONG)buf.Size();

    header = (CLR_RECORD_ASSEMBLY*)buf.GetRecordAt( 0 );
    *header = headerSrc;

    header->ComputeCRC();

    TINYCLR_NOCLEANUP();
}


HRESULT WatchAssemblyBuilder::Linker::Generate( CQuickRecord<BYTE>& buf, bool patch_fReboot, bool patch_fSign, std::wstring* patch_szNative )
{
    TINYCLR_HEADER();

    CLR_RECORD_ASSEMBLY header; TINYCLR_CLEAR(header);

                                                           // CLR_UINT8          marker[8];
                                                           //
                                                           // CLR_UINT32         headerCRC;
                                                           // CLR_UINT32         assemblyCRC;
                                                           //
                                                           // CLR_UINT32         flags;
                                                           //
                                                           // CLR_UINT32         nativeMethodsChecksum;
                                                           // CLR_UINT32         patchEntryOffset;
                                                           //
                                                           // CLR_RECORD_VERSION version;
                                                           //
                                                           // CLR_STRING         assemblyName;    // TBL_Strings
    header.stringTableVersion = c_CLR_StringTable_Version; // CLR_UINT16         stringTableVersion;
                                                           //
                                                           // CLR_OFFSET_LONG    startOfTables[TBL_Max];

    if(m_pr)
    {
        header.version = m_pr->m_version;

        if(!AllocString( m_pr->m_assemblyName, header.assemblyName, false )) REPORT_NO_MEMORY();
    }

    if(patch_fReboot)
    {
        header.flags |= CLR_RECORD_ASSEMBLY::c_Flags_NeedReboot;
    }

    if(patch_szNative)
    {
        CLR_RT_Buffer buffer;

        TINYCLR_CHECK_HRESULT(CLR_RT_FileStore::LoadFile( patch_szNative->c_str(), buffer ));

        header.patchEntryOffset = (CLR_UINT32)m_tableResourceData.GetPos();

        BYTE* res = m_tableResourceData.Alloc( buffer.size() ); if(res == NULL) REPORT_NO_MEMORY();

        memcpy( res, &buffer[0], buffer.size() );

        TINYCLR_CHECK_HRESULT(AlignToWORD( m_tableResourceData ));
    }
    else
    {
        header.patchEntryOffset = 0xFFFFFFFF;
    }


    //--//

    TINYCLR_CHECK_HRESULT(EmitData( buf, header ));

    //--//

    if(patch_fSign)
    {
        RSAKey key =
        {
            RSA_KEY_SIZE_BYTES / sizeof(DWORD),
            {
                0x67, 0xF6, 0x23, 0x10, 0xD5, 0x29, 0x22, 0xF6,
                0x01, 0xB2, 0x74, 0x7D, 0x98, 0x9D, 0xBF, 0x10,
                0xCA, 0xF4, 0x24, 0x73, 0xF5, 0x1A, 0x77, 0xF3,
                0xAE, 0x6D, 0xDD, 0x6F, 0x8C, 0x37, 0xAD, 0x27,
                0x7F, 0x78, 0x02, 0x76, 0x3A, 0x8C, 0xAA, 0xC1,
                0xAA, 0xF8, 0x53, 0x10, 0x26, 0xF7, 0xA3, 0x23,
                0xC9, 0x61, 0xDF, 0x53, 0xCC, 0x62, 0x40, 0x82,
                0x83, 0x38, 0x7C, 0xCD, 0x99, 0x31, 0x7D, 0x9B,
                0x35, 0xDD, 0xF9, 0xEE, 0xFF, 0xF4, 0x84, 0xB9,
                0x9D, 0x11, 0x1D, 0x07, 0x9D, 0x9B, 0x92, 0xEE,
                0x1F, 0xEF, 0xD8, 0x96, 0x36, 0x9A, 0xAB, 0x68,
                0xA3, 0x07, 0x9A, 0x6F, 0x8D, 0x58, 0x96, 0x0E,
                0xFE, 0x54, 0xAB, 0x4E, 0x13, 0x91, 0x6C, 0xDA,
                0x58, 0x17, 0x2C, 0x07, 0x5F, 0x07, 0xAA, 0x55,
                0x67, 0x34, 0x4D, 0x11, 0xEF, 0xC5, 0x1F, 0x18,
                0xA0, 0xCC, 0xB4, 0x6B, 0xD0, 0x9D, 0xC3, 0xD0
            },
            {
                0xA1, 0xF0, 0xA3, 0x34, 0x6A, 0x0A, 0xFF, 0x3A,
                0x63, 0x4E, 0xC3, 0x55, 0xCE, 0x56, 0x10, 0x19,
                0x1F, 0xD1, 0xBE, 0x3E, 0x3E, 0x94, 0xCC, 0x1C,
                0xD0, 0x9E, 0xB6, 0x2E, 0x36, 0x8F, 0x2F, 0x56,
                0x76, 0x6E, 0x50, 0x79, 0xC9, 0xA6, 0xEA, 0xC1,
                0x99, 0xFC, 0x46, 0xB4, 0xB3, 0xBE, 0x49, 0x1D,
                0x3A, 0x62, 0x2F, 0x23, 0x44, 0x41, 0x35, 0xA2,
                0x24, 0xD1, 0xCA, 0x04, 0x12, 0xAB, 0x20, 0x55,
                0x90, 0x70, 0xF0, 0x03, 0xDF, 0x9D, 0x43, 0x67,
                0x7F, 0xD4, 0x64, 0x74, 0x61, 0x49, 0xAE, 0x04,
                0x60, 0xDC, 0x39, 0x4A, 0x70, 0x75, 0xE5, 0x03,
                0x13, 0xC4, 0x26, 0x79, 0x29, 0x0A, 0x8F, 0x8E,
                0xE6, 0xCD, 0xA9, 0x86, 0x64, 0x2A, 0x31, 0xAF,
                0x2D, 0xE4, 0x8C, 0x8F, 0x6D, 0x75, 0x6B, 0xD1,
                0x3E, 0x9D, 0xE8, 0xB8, 0xCE, 0xB2, 0xCB, 0x17,
                0xB9, 0x68, 0xD6, 0xB8, 0x7C, 0x4B, 0x53, 0x02
            },
        };

        UINT8 result[RSA_BLOCK_SIZE_BYTES];

        if(CRYPTO_SUCCESS == ::Crypto_SignBuffer( (BYTE*)buf.Ptr(), (int)buf.Size(), &key, result, sizeof(result) ))
        {
            RSAKey key2;

            if(CRYPTO_SUCCESS == ::Crypto_PublicKeyFromModulus( &key.module, &key2 ))
            {
                BYTE unmanagedHash     [HASH_SIZE];
                BYTE decryptedSignature[HASH_SIZE];

                if(::Crypto_GetHash( (BYTE*)buf.Ptr(), (int)buf.Size(), unmanagedHash, HASH_SIZE ))
                {
                    if(CRYPTO_SUCCESS == ::Crypto_RSADecrypt( &key2, decryptedSignature, HASH_SIZE, result, sizeof(result) ))
                    {
                        if(memcmp( decryptedSignature, unmanagedHash, HASH_SIZE ) == 0)
                        {
                            int i = 1;
                        }
                    }
                }
            }

            void* ptr = buf.Alloc( sizeof(result) );

            memcpy( ptr, result, sizeof(result) );
        }
    }

    //--//

    TINYCLR_NOCLEANUP();
}

HRESULT WatchAssemblyBuilder::Linker::DumpPdbxToken( CLR_XmlUtil& xml, IXMLDOMNodePtr pNodeParent, mdToken tk )
{
    TINYCLR_HEADER();

    IXMLDOMNodePtr pNodeToken;
    bool           fFound;

    TINYCLR_CHECK_HRESULT(xml.CreateNode( L"Token"  , &pNodeToken, pNodeParent ));
    TINYCLR_CHECK_HRESULT(xml.PutValue  ( L"CLR"    , ToHex( tk              ), fFound,  pNodeToken              ));
    TINYCLR_CHECK_HRESULT(xml.PutValue  ( L"TinyCLR", ToHex( m_lookupIDs[tk] ), fFound,  pNodeToken              ));

    TINYCLR_NOCLEANUP();
}

HRESULT WatchAssemblyBuilder::Linker::DumpPdbx( LPCWSTR szFileNamePE )
{
    TINYCLR_HEADER();

    CLR_XmlUtil    xml;
    IXMLDOMNodePtr pNodePDBXFile;
    IXMLDOMNodePtr pNodeAssembly;
    IXMLDOMNodePtr pNodeClasses;
    IXMLDOMNodePtr pNodeVersion;
    std::wstring   strFilePdbx;
    std::wstring   strAssemblyFile;
    bool           fFound;

    if(!m_pr) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

    TINYCLR_CHECK_HRESULT(GetFullPath( m_pr->m_assemblyFile, NULL, &strAssemblyFile ));

    TINYCLR_CHECK_HRESULT(xml.New( L"PdbxFile" ));
    xml.GetRoot( &pNodePDBXFile );

    xml.CreateNode( L"Assembly", &pNodeAssembly, pNodePDBXFile );
    DumpPdbxToken( xml, pNodeAssembly, m_pr->m_tkAsm );


    xml.PutValue( L"FileName", strAssemblyFile, fFound, pNodeAssembly );

#ifdef DEBUG
    xml.PutValue( L"Name"   , m_pr->m_assemblyName, fFound, pNodeAssembly );
#endif

    xml.CreateNode( L"Version", &pNodeVersion, pNodeAssembly );

    xml.PutValue( L"Major"   , _variant_t( m_pr->m_version.iMajorVersion   ), fFound, pNodeVersion );
    xml.PutValue( L"Minor"   , _variant_t( m_pr->m_version.iMinorVersion   ), fFound, pNodeVersion );
    xml.PutValue( L"Build"   , _variant_t( m_pr->m_version.iBuildNumber    ), fFound, pNodeVersion );
    xml.PutValue( L"Revision", _variant_t( m_pr->m_version.iRevisionNumber ), fFound, pNodeVersion );

    xml.CreateNode( L"Classes", &pNodeClasses, pNodeAssembly );

    for(MetaData::TypeDefMapIter itTypeDef = m_pr->m_mapDef_Type.begin(); itTypeDef != m_pr->m_mapDef_Type.end(); itTypeDef++)
    {
        MetaData::TypeDef&   td    = itTypeDef->second;
        CLR_RECORD_TYPEDEF*  tdCLR = m_tableTypeDef.GetRecordAt( CLR_DataFromTk( m_lookupIDs[td.m_td] ) );

        IXMLDOMNodePtr pNodeClass;
        IXMLDOMNodePtr pNodeFields;
        IXMLDOMNodePtr pNodeMethods;

        xml.CreateNode( L"Class",                    &pNodeClass, pNodeClasses );

#ifdef DEBUG
        xml.PutValue  ( L"Name" , td.m_name, fFound,  pNodeClass               );

        switch(tdCLR->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask)
        {
        case CLR_RECORD_TYPEDEF::TD_Semantics_ValueType:
            xml.PutValue( L"IsValueClass", L"true", fFound, pNodeClass );
            break;

        case CLR_RECORD_TYPEDEF::TD_Semantics_Enum:
            xml.PutValue( L"IsEnum", L"true", fFound, pNodeClass );
            break;
        }
#endif

        DumpPdbxToken( xml, pNodeClass, td.m_td );

        //--//

        xml.CreateNode( L"Methods", &pNodeMethods, pNodeClass );

        for(MetaData::mdMethodDefListIter itMethod = td.m_methods.begin(); itMethod != td.m_methods.end(); itMethod++)
        {
            MetaData::MethodDef&  md    = m_pr->m_mapDef_Method.find( *itMethod )->second;
            CLR_RECORD_METHODDEF* mdCLR = m_tableMethodDef.GetRecordAt( CLR_DataFromTk( m_lookupIDs[md.m_md] ) );

            IXMLDOMNodePtr pNodeMethod;
            IXMLDOMNodePtr pNodeILMap;
            int            ipDiff = 0;

            if(md.m_byteCode.m_opcodes.size() != md.m_byteCodeOriginal.m_opcodes.size()) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

            xml.CreateNode( L"Method",                                                  &pNodeMethod, pNodeMethods );
#ifdef DEBUG
            xml.PutValue  ( L"Name"  ,   md.m_name,                             fFound,  pNodeMethod               );
            xml.PutValue  ( L"NumArg",   _variant_t( mdCLR->numArgs         ), fFound,  pNodeMethod               );
            xml.PutValue  ( L"NumLocal", _variant_t( mdCLR->numLocals       ), fFound,  pNodeMethod               );
            xml.PutValue  ( L"MaxStack", _variant_t( mdCLR->lengthEvalStack ), fFound,  pNodeMethod               );
#endif
            DumpPdbxToken( xml, pNodeMethod, md.m_md );

            if(!md.m_byteCode.m_opcodes.size())
            {
                xml.PutValue( L"HasByteCode", L"false", fFound, pNodeMethod );
            }

            xml.CreateNode( L"ILMap", &pNodeILMap, pNodeMethod );

            for(size_t i=0; i<md.m_byteCode.m_opcodes.size(); i++)
            {
                IXMLDOMNodePtr                         pNodeIL;
                MetaData::ByteCode::LogicalOpcodeDesc& op         = md.m_byteCode        .m_opcodes[i];
                MetaData::ByteCode::LogicalOpcodeDesc& opOriginal = md.m_byteCodeOriginal.m_opcodes[i];
                int                                    ipDiffNew  = opOriginal.m_ipOffset - op.m_ipOffset;

                if(op.m_op != opOriginal.m_op || ipDiffNew < ipDiff) TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);

                if(ipDiffNew > ipDiff)
                {
                    ipDiff = ipDiffNew;

                    xml.CreateNode( L"IL"     ,                                         &pNodeIL, pNodeILMap );
                    xml.PutValue  ( L"CLR"    , ToHex( opOriginal.m_ipOffset ), fFound,  pNodeIL             );
                    xml.PutValue  ( L"TinyCLR", ToHex( op        .m_ipOffset ), fFound,  pNodeIL             );
                }
            }
        }

        //--//

        xml.CreateNode( L"Fields", &pNodeFields, pNodeClass );

        for(MetaData::mdFieldDefListIter itField = td.m_fields.begin(); itField != td.m_fields.end(); itField++)
        {
            MetaData::FieldDef&  fd = m_pr->m_mapDef_Field.find( *itField )->second;
            IXMLDOMNodePtr       pNodeField;

            xml.CreateNode( L"Field",                    &pNodeField, pNodeFields );
#ifdef DEBUG
            xml.PutValue  ( L"Name" , fd.m_name, fFound,  pNodeField              );
#endif
            DumpPdbxToken( xml, pNodeField, fd.m_fd );
        }
    }

    ChangeExtensionOnFileName( szFileNamePE, strFilePdbx, L"pdbx" );

    TINYCLR_CHECK_HRESULT(xml.Save( strFilePdbx.c_str() ));

    TINYCLR_NOCLEANUP();
}

//--//

static HRESULT local_DumpVersion( CLR_XmlUtil& xml, IXMLDOMNode* pNode, LPCWSTR szName, CLR_RECORD_VERSION& ver )
{
    TINYCLR_HEADER();

    IXMLDOMNodePtr pNodeVersion;
    bool           fFound;

    TINYCLR_CHECK_HRESULT(xml.CreateNode( szName, &pNodeVersion, pNode ));

    TINYCLR_CHECK_HRESULT(xml.PutValue( L"Major"   , _variant_t( ver.iMajorVersion   ), fFound, pNodeVersion ));
    TINYCLR_CHECK_HRESULT(xml.PutValue( L"Minor"   , _variant_t( ver.iMinorVersion   ), fFound, pNodeVersion ));
    TINYCLR_CHECK_HRESULT(xml.PutValue( L"Build"   , _variant_t( ver.iBuildNumber    ), fFound, pNodeVersion ));
    TINYCLR_CHECK_HRESULT(xml.PutValue( L"Revision", _variant_t( ver.iRevisionNumber ), fFound, pNodeVersion ));

    TINYCLR_NOCLEANUP();
}

HRESULT WatchAssemblyBuilder::Linker::DumpDownloads( LPCWSTR szFileNamePE )
{
    TINYCLR_HEADER();

    CLR_XmlUtil xml;
    bool        fFound;

    TINYCLR_CHECK_HRESULT(xml.New( L"SpotDownloadDescriptor" ));

    if(m_pr)
    {
        IXMLDOMNodePtr pNodeAssembly;
        std::wstring   strPath;

        TINYCLR_CHECK_HRESULT(xml.CreateNode( L"Assembly", &pNodeAssembly ));

        TINYCLR_CHECK_HRESULT(xml.PutValue( L"Name", m_pr->m_assemblyName, fFound, pNodeAssembly ));

        TINYCLR_CHECK_HRESULT(local_DumpVersion( xml, pNodeAssembly, L"Version", m_pr->m_version ));

        TINYCLR_CHECK_HRESULT(GetFullPath ( m_pr->m_assemblyFile, &strPath, NULL                  ));
        TINYCLR_CHECK_HRESULT(xml.PutValue( L"Source",             strPath, fFound, pNodeAssembly ));

        TINYCLR_CHECK_HRESULT(GetFullPath ( szFileNamePE,   &strPath, NULL                  ));
        TINYCLR_CHECK_HRESULT(xml.PutValue( L"Destination",  strPath, fFound, pNodeAssembly ));

        {
            IXMLDOMNodePtr pNodeClasses;

            TINYCLR_CHECK_HRESULT(xml.CreateNode( L"Publish", &pNodeClasses, pNodeAssembly ));

            for(MetaData::TypeDefMapIter itTypeDef = m_pr->m_mapDef_Type.begin(); itTypeDef != m_pr->m_mapDef_Type.end(); itTypeDef++)
            {
                MetaData::TypeDef& td = itTypeDef->second;

                if(MetaData::IsTokenPresent( m_pr->m_setAttributes_Types_PublishInApplicationDirectory, td.m_td ))
                {
                    IXMLDOMNodePtr pNodeClass;
                    std::wstring   str;

                    m_pr->TokenToString( td.m_td, str );

                    TINYCLR_CHECK_HRESULT(xml.CreateNode( L"Class",              &pNodeClass, pNodeClasses ));
                    TINYCLR_CHECK_HRESULT(xml.PutValue  ( L"Name" , str, fFound,  pNodeClass               ));
                }
            }
        }
    }
    else
    {
        TINYCLR_SET_AND_LEAVE(CLR_E_FAIL);
    }

    {
        std::wstring strFile;

        ChangeExtensionOnFileName( szFileNamePE, strFile, L"pe_downloads" );

        TINYCLR_CHECK_HRESULT(xml.Save( strFile.c_str() ));
    }

    TINYCLR_NOCLEANUP();
}
