////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#pragma once

#include <AssemblyParser.h>

#include <TinyCLR_Runtime.h>

namespace WatchAssemblyBuilder
{
    LPCWSTR ToHex( CLR_UINT32 u );

    void ChangeExtensionOnFileName( std::wstring strFile, std::wstring& strFileNew, LPWSTR szExt );

    template <class T> class CQuickRecord : public CQuickBytesBase
    {
        SIZE_T m_pos;

    public:
        CQuickRecord()
        {
            m_pos = 0;
            
            Init();
        }

        ~CQuickRecord()
        {
            Destroy();
        }

        void Destroy()
        {
            m_pos = 0;

            CQuickBytesBase::Destroy();

            Init();
        }

        bool Queue( T b )
        {
            T* res = Alloc( 1 ); if(res == NULL) return false;

            *res = b;

            return true;
        }

        T* Alloc( SIZE_T num )
        {
            if(FAILED(ReSizeNoThrow( (m_pos + num) * sizeof(T) ))) return NULL;

            T* res = &((T*)Ptr())[ m_pos ];

            CLR_RT_Memory::ZeroFill( res, num * sizeof(T) );

            m_pos += num;

            return res;
        }

        T* GetRecordAt( SIZE_T num )
        {
            if(num > m_pos) return NULL;

            return &((T*)Ptr())[ num ];
        }

        HRESULT CopyTo( CQuickRecord<BYTE>& output, CLR_OFFSET_LONG& offset, size_t maxSize = 0xFFFFFFFF )
        {
            TINYCLR_HEADER();

            offset = (CLR_OFFSET_LONG)output.GetPos();

            size_t len = Size(); if(len >= maxSize) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
            BYTE*  res = output.Alloc( len );
            if(res == NULL)
            {
                TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_MEMORY);
            }

            memcpy( res, Ptr(), len );

            TINYCLR_NOCLEANUP();
        }

        SIZE_T GetPos()
        {
            return m_pos;
        }

        void Reset()
        {
            m_pos = 0;
        }

        HRESULT VerifyRange( SIZE_T base, SIZE_T width, LPCWSTR szItem, LPCWSTR szKind )
        {
            TINYCLR_HEADER();

            if(m_pos < base || m_pos >= base + width)
            {
                wprintf( L"Exceeded maximum %s (%d) of %s: %d\n", szItem, width, szKind, m_pos - base );

                TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
            }

            TINYCLR_NOCLEANUP();
        }
    };

    class Linker
    {
        friend MetaData::CustomAttribute::Writer;

        class CustomAttributeId
        {
            friend class Linker;

            MetaData::CustomAttribute& m_ca;
            CLR_RECORD_ATTRIBUTE       m_id;

        public:
            CustomAttributeId( Linker* lk, MetaData::CustomAttribute& ca );

            bool operator<( const CustomAttributeId& r ) const;
        };

        typedef std::set< CustomAttributeId >  CustomAttributeIdSet;
        typedef CustomAttributeIdSet::iterator CustomAttributeIdSetIter;

        //--//

        typedef std::map< CLR_INT32, mdToken > IPTokenMap;
        typedef IPTokenMap::iterator           IPTokenMapIter;

        //--//

        class ExceptionHandlerHierarchy
        {
            CLR_RECORD_EH*                        m_data;
            std::list<ExceptionHandlerHierarchy*> m_children;

            //--//

            ExceptionHandlerHierarchy( const CLR_RECORD_EH& eh );

            void Queue( ExceptionHandlerHierarchy* newEhh );

            bool FindMaxCoverage( CLR_OFFSET& start, CLR_OFFSET& end ) const;

        public:
            ExceptionHandlerHierarchy();
            ExceptionHandlerHierarchy( ExceptionHandlerHierarchy& ehh );
            ExceptionHandlerHierarchy& operator=( const ExceptionHandlerHierarchy& ehh );
            ~ExceptionHandlerHierarchy();

            //--//

            void Clear();

            void Queue( const CLR_RECORD_EH& eh );

            HRESULT GenerateOutput( CQuickRecord<CLR_RECORD_EH>& tbl );
        };

        friend class CustomAttributeId;

        CQuickRecord<CLR_RECORD_ASSEMBLYREF   > m_tableAssemblyRef ;
        CQuickRecord<CLR_RECORD_TYPEREF       > m_tableTypeRef     ;
        CQuickRecord<CLR_RECORD_FIELDREF      > m_tableFieldRef    ;
        CQuickRecord<CLR_RECORD_METHODREF     > m_tableMethodRef   ;
        CQuickRecord<CLR_RECORD_TYPEDEF       > m_tableTypeDef     ;
        CQuickRecord<CLR_RECORD_FIELDDEF      > m_tableFieldDef    ;
        CQuickRecord<CLR_RECORD_METHODDEF     > m_tableMethodDef   ;
        CQuickRecord<CLR_RECORD_ATTRIBUTE     > m_tableAttribute   ;
        CQuickRecord<CLR_RECORD_TYPESPEC      > m_tableTypeSpec    ;
        CQuickRecord<CLR_RECORD_RESOURCE      > m_tableResource    ;
        CQuickRecord<CLR_RECORD_RESOURCE_FILE > m_tableResourceFile;
        CQuickRecord<BYTE                     > m_tableResourceData;
        CQuickRecord<CHAR                     > m_tableString      ;
        CQuickRecord<BYTE                     > m_tableSignature   ;
        CQuickRecord<BYTE                     > m_tableByteCode    ;

        std::set<std::string>                m_collectUniqueStrings;

        std::map<std::string,CLR_OFFSET>     m_lookupStringsConst;
        std::map<std::string,CLR_OFFSET>     m_lookupStrings;
        MetaData::mdTokenMap                 m_lookupIDs;
        MetaData::mdTokenMap                 m_posString;

        MetaData::mdTokenSet                 m_setAttributes_Types;
        MetaData::mdTokenSet                 m_setAttributes_Fields;
        MetaData::mdTokenSet                 m_setAttributes_Methods;

        MetaData::Parser*                    m_pr;

        BYTE                                 m_tmpSig[ 1024 ];
        BYTE*                                m_tmpSigPtr;
        BYTE*                                m_tmpSigEnd;

        bool AllocString( const std::string&  str                                     , CLR_STRING& idx                        , bool fUser );
        bool AllocString( const std::wstring& str                                     , CLR_STRING& idx                        , bool fUser );
        bool AllocString( const std::wstring& str                                     , CLR_STRING& name, CLR_STRING& nameSpace             );
        bool AllocString( const std::string&  strName, const std::string& strNameSpace, CLR_STRING& name, CLR_STRING& nameSpace             );

        bool CheckDuplicateOrAppend( CLR_IDX&    idx   , BYTE* dst, size_t dstLen, const BYTE* src, size_t srcLen, size_t align, LPCWSTR szText );
        bool CheckDuplicateOrAppend( CLR_UINT32& offset, BYTE* dst, size_t dstLen, const BYTE* src, size_t srcLen, size_t align, LPCWSTR szText );

        void PrepareSignature           (                                                                                                           );
        bool GenerateSignature          ( MetaData::TypeSignature               * sig                                                               );
        bool GenerateSignature          ( MetaData::CustomAttribute             & ca                                                                );
        bool GenerateSignatureData8     (                                              CLR_UINT8                      val                           );
        bool GenerateSignatureData32    (                                              CLR_UINT32                     val                           );
        bool GenerateSignatureToken     (                                              mdToken                        tk                            );
        bool FlushSignature             (                                              CLR_SIG&                       idx, const BYTE* ptr, int len );
        bool FlushSignature             (                                              CLR_SIG&                       idx                           );
        bool AllocSignatureForField     ( MetaData::TypeSignature               * sig, CLR_SIG&                       idx                           );
        bool AllocSignatureForMethod    ( MetaData::MethodSignature             * sig, CLR_SIG&                       idx                           );
        bool AllocSignatureForAttribute ( MetaData::CustomAttribute             & ca , CLR_SIG&                       idx                           );
        bool AllocSignatureForTypeSpec  ( MetaData::TypeSignature               * sig, CLR_SIG&                       idx                           );
        bool AllocSignatureForLocalVar  ( MetaData::LocalVarSignature           & sig, CLR_SIG&                       idx                           );
        bool AllocSignatureForInterfaces( MetaData::mdInterfaceImplList         & sig, CLR_SIG&                       idx                           );
        
        HRESULT ValidateTypeRefOrDef( mdToken                        tk, bool fAsInterface );
        CLR_IDX EncodeTypeRefOrDef  ( mdToken                        tk                    );
        CLR_IDX EncodeToken         ( mdToken                        tk                    );

        HRESULT ResolveTypeRef( MetaData::TypeRef&           tr, MetaData::mdTypeRefList&              order, MetaData::mdTokenSet& resolved );
        HRESULT ResolveTypeDef( MetaData::TypeDef&           td, MetaData::mdTypeRefList&              order, MetaData::mdTokenSet& resolved );

        HRESULT ProcessAssemblyRef       (                                                                                                     );
        HRESULT ProcessTypeRef           (                                                                                                     );
        HRESULT ProcessMemberRef         (                                                                                                     );
        HRESULT ProcessTypeDef           ( MetaData::mdTypeDefList& order                                                                      );
        HRESULT ProcessTypeDef           ( mdTypeDef                tdIdx                                                                      );
        HRESULT ProcessTypeDef_ByteCode  ( mdTypeDef                tdIdx                                                                      );
        HRESULT ProcessFieldDef          ( MetaData::TypeDef&       td   , CLR_RECORD_TYPEDEF* tdDst, MetaData::FieldDef&  fd, CLR_UINT32 mode );
        HRESULT ProcessMethodDef         ( MetaData::TypeDef&       td   , CLR_RECORD_TYPEDEF* tdDst, MetaData::MethodDef& md, CLR_UINT32 mode );
        HRESULT ProcessMethodDef_ByteCode( MetaData::TypeDef&       td   , CLR_RECORD_TYPEDEF* tdDst, MetaData::MethodDef& md, CLR_UINT32 mode );
        HRESULT ProcessTypeSpec          (                                                                                                     );
        HRESULT ProcessAttribute         (                                                                                                     );
        HRESULT ProcessResource          (                                                                                                     );
        HRESULT ProcessUserString        (                                                                                                     );

        HRESULT EmitData( CQuickRecord<BYTE>& buf, CLR_RECORD_ASSEMBLY& headerSrc );

        HRESULT DumpPdbxToken( CLR_XmlUtil& xml, IXMLDOMNodePtr pNodeParent, mdToken tk );

    private:
        bool    m_fBigEndianTarget;

    public:
        Linker();
        ~Linker();

        //--//

        void Clean();

        HRESULT Process  ( MetaData::Parser&             pr  );

        HRESULT Generate( CQuickRecord<BYTE>& buf, bool patch_fReboot, bool patch_fSign, std::wstring* patch_szNative );

        HRESULT SaveUniqueStrings( const std::wstring& file );
        HRESULT LoadUniqueStrings( const std::wstring& file );
        HRESULT DumpUniqueStrings( const std::wstring& file );
        HRESULT DumpPdbx         ( LPCWSTR szFileNamePE );
        HRESULT DumpDownloads    ( LPCWSTR szFileNamePE );

        void LoadGlobalStrings();
		bool GetBigEndianTarget() { return m_fBigEndianTarget; }
		bool SetBigEndianTarget(bool isBE ) { m_fBigEndianTarget = isBE; return isBE; }
		
        static CLR_DataType MapElementTypeToDataType( CorElementType et );
    };
};

class ErrorReporting
{
public:
    static void Print( LPCWSTR szOrigin, LPCWSTR szSubCategory, BOOL fError, int code, LPCWSTR szTextFormat, ...);
    static HRESULT ConstructErrorOrigin( std::wstring &str, ISymUnmanagedReader* pSymReader, mdMethodDef md, ULONG32 ipOffset );
};

struct TinyResourcesFileHeader
{
    static const CLR_UINT32 MAGIC_NUMBER = 0xf995b0a8;

    CLR_UINT32 magicNumber;
    CLR_UINT32 version;
    CLR_UINT32 sizeOfHeader;
    CLR_UINT32 sizeOfResourceHeader;
    CLR_UINT32 numberOfResources;
};

struct TinyResourcesResourceHeader
{
    //
    // Sorted on id
    //
    CLR_INT16  id;
    CLR_UINT8  kind;
    CLR_UINT8  pad;
    CLR_UINT32 size;
};
