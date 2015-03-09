////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#pragma once

#include <TinyCLR_Runtime.h>

typedef LPCSTR  LPCUTF8;
typedef LPSTR   LPUTF8;


#include <cor.h>
#include <corhdr.h>
#include <corhlpr.h>
#include <corsym.h>

_COM_SMRT_PTR(ISymUnmanagedReader);
_COM_SMRT_PTR(ISymUnmanagedBinder);
_COM_SMRT_PTR_2(IMetaDataDispenserEx,IID_IMetaDataDispenserEx);
_COM_SMRT_PTR_2(IMetaDataImport, IID_IMetaDataImport);
_COM_SMRT_PTR_2(IMetaDataImport2, IID_IMetaDataImport2);
_COM_SMRT_PTR_2(IMetaDataAssemblyImport, IID_IMetaDataAssemblyImport);

//////////////////////////////////////////////////////

namespace WatchAssemblyBuilder
{
    class Linker;
};

//--//

class PELoader
{
    HANDLE            m_hFile;
    HANDLE            m_hMapFile;
    HMODULE           m_hMod;
    PIMAGE_NT_HEADERS m_pNT;

    HRESULT Initialize();

public:
    PELoader();
    ~PELoader();

    PELoader( const PELoader& pe );
    PELoader& operator= ( const PELoader& pe );

    HRESULT OpenAndMapToMemory( const WCHAR* moduleNameIn );
    HRESULT OpenAndDecode     ( const WCHAR* moduleNameIn );

    bool GetCOMHeader(               IMAGE_COR20_HEADER*& pCorHeader              );
    bool GetResource ( DWORD offset, BYTE*&               pResource, DWORD& iSize );
    bool GetVAforRVA ( DWORD rva   , void*&               va                      );

    void Close();

    PIMAGE_NT_HEADERS NtHeaders () { return        m_pNT;   }
    BYTE*             Base      () { return (BYTE*)m_hMod;  }
    HMODULE           GetHModule() { return        m_hMod;  }
    HANDLE            GetHFile  () { return        m_hFile; }

private:
    void InitToZero();
};

//--//

namespace MetaData
{
    typedef std::set<mdToken>             mdTokenSet;
    typedef mdTokenSet::iterator          mdTokenSetIter;

    typedef std::list<mdToken>            mdTokenList;
    typedef mdTokenList::iterator         mdTokenListIter;

    typedef std::map<mdToken,int>         mdTokenMap;
    typedef mdTokenMap::iterator          mdTokenMapIter;

    typedef std::list<mdTypeRef>          mdTypeRefList;
    typedef mdTypeRefList::iterator       mdTypeRefListIter;

    typedef std::list<mdMemberRef>        mdMemberRefList;
    typedef mdMemberRefList::iterator     mdMemberRefListIter;

    typedef std::list<mdTypeDef>          mdTypeDefList;
    typedef mdTypeDefList::iterator       mdTypeDefListIter;

    typedef std::list<mdFieldDef>         mdFieldDefList;
    typedef mdFieldDefList::iterator      mdFieldDefListIter;

    typedef std::list<mdMethodDef>        mdMethodDefList;
    typedef mdMethodDefList::iterator     mdMethodDefListIter;

    typedef std::list<mdInterfaceImpl>    mdInterfaceImplList;
    typedef mdInterfaceImplList::iterator mdInterfaceImplListIter;

    typedef std::list<int>                LimitList;
    typedef LimitList::iterator           LimitListIter;

    class Parser;
    class Collection;

    struct TypeDef;
    struct MethodDef;

    //--//

    void SetReference( MetaData::mdTokenSet& m, mdToken d );

    template <class K> bool IsTokenPresent( std::set<K>& d, mdToken tk )
    {
        if(IsNilToken(tk)) return true;

        return d.find( tk ) != d.end();
    }

    template <class T> bool IsTokenPresent( T& d, mdToken tk )
    {
        if(IsNilToken(tk)) return true;

        return d.find( tk ) != d.end();
    }

    //--//

    struct ByteCode
    {
        struct LogicalOpcodeDesc
        {
            const CLR_RT_OpcodeLookup* m_ol;
            CLR_OPCODE                 m_op;

            CLR_UINT32                 m_ipOffset;
            CLR_UINT32                 m_ipLength;

            CLR_UINT32                 m_stackDepth;
            CLR_INT32                  m_stackDiff;

            CLR_UINT32                 m_references;

            CLR_UINT32                 m_index;
            mdToken                    m_token;
            CLR_INT32                  m_arg_I4;
            CLR_INT32                  m_arg_R4;
            CLR_INT64                  m_arg_I8;
            CLR_INT64                  m_arg_R8;
            std::vector<CLR_INT32>     m_targets;

            LogicalOpcodeDesc( const CLR_RT_OpcodeLookup& ol, CLR_OPCODE op, const UINT8* ip, const UINT8* ipEnd );
        };

        struct LogicalExceptionBlock
        {
            CorExceptionFlag m_Flags;
            CLR_INT32        m_TryIndex;
            CLR_INT32        m_TryIndexEnd;
            CLR_UINT32       m_TryOffset;
            CLR_UINT32       m_TryLength;
            CLR_INT32        m_HandlerIndex;
            CLR_INT32        m_HandlerIndexEnd;
            CLR_UINT32       m_HandlerOffset;
            CLR_UINT32       m_HandlerLength;
            mdToken          m_ClassToken;
            CLR_UINT32       m_FilterOffset;
            CLR_INT32        m_FilterIndex;
        };

        typedef std::map<CLR_INT32,CLR_INT32>               OffsetToIndex;
        typedef OffsetToIndex::iterator                     OffsetToIndexIter;
        typedef OffsetToIndex::const_iterator               OffsetToIndexConstIter;

        typedef std::vector<LogicalOpcodeDesc>              LogicalOpcodeDescVector;
        typedef LogicalOpcodeDescVector::iterator           LogicalOpcodeDescVectorIter;
        typedef LogicalOpcodeDescVector::const_iterator     LogicalOpcodeDescVectorConstIter;

        typedef std::vector<LogicalExceptionBlock>          LogicalExceptionBlockVector;
        typedef LogicalExceptionBlockVector::iterator       LogicalExceptionBlockVectorIter;
        typedef LogicalExceptionBlockVector::const_iterator LogicalExceptionBlockVectorConstIter;

        typedef std::map<size_t,size_t>                     Distribution;
        typedef Distribution::iterator                      DistributionIter;
        typedef Distribution::const_iterator                DistributionConstIter;

        //--//

        std::wstring                m_name;
        LogicalOpcodeDescVector     m_opcodes;
        LogicalExceptionBlockVector m_exceptions;

        static Distribution s_numOfOpcodes;
        static Distribution s_numOfEHs;
        static Distribution s_sizeOfMethod;

        //--//

        HRESULT Parse            ( const TypeDef& td, const MethodDef& md, COR_ILMETHOD_DECODER& il );
        HRESULT VerifyConsistency( const TypeDef& td, const MethodDef& md, Parser*               pr );

        //--//

        HRESULT ConvertTokens( mdTokenMap&        lookupIDs             );
        HRESULT GenerateOldIL( std::vector<BYTE>& code, bool fBigEndian );

        CLR_UINT32 MaxStackDepth();

        //--//

        void DumpStats();

        static void DumpDistributionStats();

    private:
        HRESULT UpdateStackDepth (                              );
        HRESULT ComputeStackDepth( size_t pos, CLR_UINT32 depth );

        LogicalOpcodeDesc* FindTarget( OffsetToIndex& map, CLR_INT32 offset, CLR_INT32& index );

        void DumpOpcode( size_t index, LogicalOpcodeDesc& ref );

        //--//

        HRESULT Parse_ByteCode( const MethodDef& md, COR_ILMETHOD_DECODER& il );
    };

    //--//

    struct TypeSignature
    {
        Parser*        m_holder;

        // Type of element.
        CorElementType m_opt;
        // It was introduced for supporting of pinned attribute on local variables. See ELEMENT_TYPE_MODIFIER
        // In theory it could be incorporated into m_opt. Then it would require modification of existing code using m_opt.
        // Also in signature for local variables type modifier inserted as separate 8 bit field. So we follow the same logic.
        CorElementType m_optTypeModifier;
        
        mdToken        m_token;
        TypeSignature* m_sub;
        int            m_rank;
        LimitList      m_sizes;
        LimitList      m_lowBounds;

        //--//

        TypeSignature( Parser* holder );

        TypeSignature( const TypeSignature& ts );
        // Assigment opertor creates new based on ts.m_sub.
        // Operator should be updated if new members are added to TypeSignature!!!
        TypeSignature& operator= ( const TypeSignature& ts );

        ~TypeSignature();

        //--//

        void ExtractTypeRef( mdTokenSet& set );

        HRESULT Parse( PCCOR_SIGNATURE&     pSigBlob                                      );
        HRESULT Parse( CLR_RT_StringVector& sig     , CLR_RT_StringVector::size_type& pos );

        bool operator==( const TypeSignature& sig ) const ;
        bool operator!=( const TypeSignature& sig ) const { return !(*this == sig); }

    private:
        void Init ();
        void Clean();

        HRESULT ParseToken  ( PCCOR_SIGNATURE&     pSigBlob                                      );
        HRESULT ParseToken  ( CLR_RT_StringVector& sig     , CLR_RT_StringVector::size_type& pos );

        HRESULT ParseSubType( PCCOR_SIGNATURE&     pSigBlob                                      );
        HRESULT ParseSubType( CLR_RT_StringVector& sig     , CLR_RT_StringVector::size_type& pos );

        HRESULT ParseSzArray( PCCOR_SIGNATURE&     pSigBlob                                      );
        HRESULT ParseSzArray( CLR_RT_StringVector& sig     , CLR_RT_StringVector::size_type& pos );

        HRESULT ParseArray  ( PCCOR_SIGNATURE&     pSigBlob                                      );
        HRESULT ParseArray  ( CLR_RT_StringVector& sig     , CLR_RT_StringVector::size_type& pos );
    };

    typedef std::list<TypeSignature>      TypeSignatureList;
    typedef TypeSignatureList::iterator   TypeSignatureIter;


    struct MethodSignature
    {
        Parser*           m_holder;

        BYTE              m_flags;
        TypeSignature     m_retValue;
        TypeSignatureList m_lstParams;

        //--//

        MethodSignature( Parser* holder );

        //--//

        void ExtractTypeRef( mdTokenSet& set );

        HRESULT Parse( PCCOR_SIGNATURE&     pSigBlob                                      );
        HRESULT Parse( CLR_RT_StringVector& sig     , CLR_RT_StringVector::size_type& pos );

        bool operator==( const MethodSignature& sig ) const ;
        bool operator!=( const MethodSignature& sig ) const { return !(*this == sig); }
    };

    struct LocalVarSignature
    {
        Parser*           m_holder;

        TypeSignatureList m_lstVars;

        //--//

        LocalVarSignature( Parser* holder );

        //--//

        void ExtractTypeRef( mdTokenSet& set );

        HRESULT Parse( PCCOR_SIGNATURE&     pSigBlob                                      );
        HRESULT Parse( CLR_RT_StringVector& sig     , CLR_RT_StringVector::size_type& pos );
    };

    struct TypeSpecSignature
    {
        Parser*           m_holder;

        mdToken           m_type;
        TypeSignature     m_sigField;
        LocalVarSignature m_sigLocal;
        MethodSignature   m_sigMethod;

        //--//

        TypeSpecSignature( Parser* holder );

        //--//

        void ExtractTypeRef( mdTokenSet& set );

        HRESULT Parse( PCCOR_SIGNATURE&     pSigBlob                                      );
        HRESULT Parse( CLR_RT_StringVector& sig     , CLR_RT_StringVector::size_type& pos );
    };

    //--//

    struct AssemblyRef
    {
        mdAssemblyRef      m_ar;
        DWORD              m_flags;
        std::wstring       m_name;
        CLR_RECORD_VERSION m_version;

        AssemblyRef();
    };

    struct ModuleRef
    {
        mdModuleRef  m_mr;
        std::wstring m_name;

        ModuleRef();
    };

    struct TypeRef
    {
        mdTypeRef       m_tr;
        std::wstring    m_name;
        mdToken         m_scope; // ResolutionScope coded index.

        mdMemberRefList m_lst;

        TypeRef();
    };

    struct MemberRef
    {
        mdToken           m_tr; // MemberRefParent coded index.
        mdToken           m_mr; // MemberRefParent coded index.
        std::wstring      m_name;
        TypeSpecSignature m_sig;

        MemberRef( Parser* holder );
    };

    struct TypeDef
    {
        mdTypeDef           m_td;
        DWORD               m_flags;
        std::wstring        m_name;
        mdToken             m_extends; // TypeDefOrRef coded index.
        mdTypeDef           m_enclosingClass;

        mdFieldDefList      m_fields;
        mdMethodDefList     m_methods;
        mdInterfaceImplList m_interfaces;

        TypeDef();
    };

    struct FieldDef
    {
        mdTypeDef         m_td;
        mdFieldDef        m_fd;
        DWORD             m_attr;
        DWORD             m_flags;
        std::wstring      m_name;
        CLR_RT_Buffer     m_value;
        TypeSpecSignature m_sig;

        FieldDef( Parser* holder );

        bool SetValue( const void* ptr, int len );
    };

    struct MethodDef
    {
        mdTypeDef         m_td;
        mdMethodDef       m_md;
        DWORD             m_implFlags;
        DWORD             m_flags;
        std::wstring      m_name;
        MethodSignature   m_method;
        LocalVarSignature m_vars;
        DWORD             m_RVA;
        const BYTE*       m_VA;
        ByteCode          m_byteCode;
        ByteCode          m_byteCodeOriginal;
        CLR_UINT32        m_maxStack;

        MethodDef( Parser* holder );
    };

    struct InterfaceImpl
    {
        mdTypeDef m_td;
        mdToken   m_itf; // TypeDefOrRef

        InterfaceImpl();
    };

    struct CustomAttribute
    {
        struct Reader
        {
            std::vector<BYTE>& m_blob;
            const BYTE*        m_pos;

            Reader( std::vector<BYTE>& blob );

            bool Read( void* dst, int size );

            bool ReadCompressedLength( int& val );

            bool Read( CorSerializationType& val );
            bool Read( CLR_UINT8&            val );
            bool Read( CLR_UINT16&           val );
            bool Read( CLR_UINT32&           val );
            bool Read( CLR_UINT64&           val );
            bool Read( std::wstring&         val );
        };

        struct Writer
        {
            WatchAssemblyBuilder::Linker* m_lk;
            BYTE*&                        m_ptr;
            BYTE*                         m_end;

            Writer( WatchAssemblyBuilder::Linker* lk, BYTE*& ptr, BYTE* end );

            bool Write( const void* dst, int size );

            bool WriteCompressedLength( int val );

            bool Write( const CorSerializationType val );
            bool Write( const CLR_UINT8            val );
            bool Write( const CLR_UINT16           val );
            bool Write( const CLR_UINT32           val );
            bool Write( const CLR_UINT64           val );
            bool Write( const std::wstring&        val );
        };

        struct Name
        {
            bool         m_fField;
            std::wstring m_text;

            bool operator<( const Name& r ) const { return m_text < r.m_text; }
        };

        union Numeric
        {
            CLR_UINT8  u1;
            CLR_UINT16 u2;
            CLR_UINT32 u4;
            CLR_UINT64 u8;
            //
            CLR_INT8   s1;
            CLR_INT16  s2;
            CLR_INT32  s4;
            CLR_INT64  s8;

            Numeric() { u8 = 0; }
        };

        struct Value
        {
            CorSerializationType m_opt;
            Numeric              m_numeric;
            std::wstring         m_text;

            //--//

            Value() { m_opt = (CorSerializationType)ELEMENT_TYPE_VOID; }

            bool Parse( Reader& reader );
            bool Emit ( Writer& writer );
        };

        typedef std::list<      Value > ValueList; typedef ValueList::iterator ValueListIter;
        typedef std::map< Name, Value > ValueMap ; typedef ValueMap ::iterator ValueMapIter;

        Parser*           m_holder;

        mdToken           m_tkObj;
        mdToken           m_tkType;
        std::vector<BYTE> m_blob;
        ValueList         m_valuesFixed;
        ValueMap          m_valuesVariable;

        std::wstring      m_nameOfAttributeClass;

        //--//

        CustomAttribute( Parser* holder );

        HRESULT Parse();
    };

    struct TypeSpec
    {
        mdTypeSpec    m_ts;
        TypeSignature m_sig;

        TypeSpec( Parser* holder );
    };

    struct ManifestResource
    {
        mdManifestResource m_mr;
        std::wstring       m_name;
        std::vector<BYTE>  m_blob;
        mdToken            m_tkImplementation;
        DWORD              m_dwResourceFlags;

        bool               m_fUsed;

        //--//

        ManifestResource( Parser* holder );
    };

    struct ParsedResource
    {
        CLR_UINT16    m_kind;
        mdToken       m_tkObj;
        CLR_UINT16    m_langId;
        CLR_UINT32    m_id;

        std::wstring  m_text;
        CLR_RT_Buffer m_blob;

        //--//

        std::wstring  m_fullName;
        bool          m_fNoCompression;
    };

    //--//
    class TypeDefMapSorted : public std::map<std::wstring, TypeDef>

    {  
        std::map<mdTypeDef, std::wstring> m_mdTokenToTypeName;

        public :

        iterator find(const mdTypeDef& mdKey) 
        { 
            // Map from type token into full type name.
            std::map<mdTypeDef, std::wstring>::iterator pName = m_mdTokenToTypeName.find( mdKey );
            
            // If there is no string for token - not found.
            if ( pName == m_mdTokenToTypeName.end() )
            {
                // Means element was not found
                return end();
            }
            // pName->second is type name. Looks element by type name
            return std::map<std::wstring, MetaData::TypeDef>::find( pName->second );
        }

        void insert( const mdTypeDef& tk, const TypeDef& td )
        {
            // Create string representation of token.
            wchar_t name_buffer[100];
            swprintf( name_buffer, ARRAYSIZE(name_buffer) - 1, L"%x",  (UINT32)tk );
            
            // Take the type name and concatinate with string representation of token.
            // This way we cover the case if type name is empty and also absolutely sure that string is unique.
            // The strings still ordered alphabetically by type name becuase it goes first.
            std::wstring strName =  td.m_name + std::wstring( name_buffer ) ;
            
            // Insert token/typename into m_mdTokenToTypeName - map of type tokens to typename strings.
            m_mdTokenToTypeName.insert( std::pair<mdTypeDef,std::wstring>( tk, strName ) );

            // Insert into typename/TypeDef map. Calls "insert" of base map class. 
            std::map<std::wstring, TypeDef>::insert( std::pair<std::wstring, TypeDef>( strName, td ) );
        }

        void RemoveByToken( mdToken tk )
        {
            // First search in the list of type names.
            std::map<mdTypeDef, std::wstring>::iterator pName = m_mdTokenToTypeName.find( tk );
            
            // If found, remove type by name.
            if ( pName != m_mdTokenToTypeName.end() )
            {
                // Removes string to type pair
                erase( pName->second );
                // Removes token to string pair
                m_mdTokenToTypeName.erase( tk );
            }
        }
    };

    typedef std::map     < mdAssemblyRef     , AssemblyRef      > AssemblyRefMap     ; typedef AssemblyRefMap     ::iterator AssemblyRefMapIter     ;
    typedef std::map     < mdModuleRef       , ModuleRef        > ModuleRefMap       ; typedef ModuleRefMap       ::iterator ModuleRefMapIter       ;
    typedef std::map     < mdTypeRef         , TypeRef          > TypeRefMap         ; typedef TypeRefMap         ::iterator TypeRefMapIter         ;
    typedef std::map     < mdMemberRef       , MemberRef        > MemberRefMap       ; typedef MemberRefMap       ::iterator MemberRefMapIter       ;

    typedef TypeDefMapSorted                                      TypeDefMap         ; typedef TypeDefMap         ::iterator TypeDefMapIter         ;
    typedef std::map     < mdFieldDef        , FieldDef         > FieldDefMap        ; typedef FieldDefMap        ::iterator FieldDefMapIter        ;
    typedef std::map     < mdMethodDef       , MethodDef        > MethodDefMap       ; typedef MethodDefMap       ::iterator MethodDefMapIter       ;
    typedef std::map     < mdInterfaceImpl   , InterfaceImpl    > InterfaceImplMap   ; typedef InterfaceImplMap   ::iterator InterfaceImplMapIter   ;

    typedef std::map     < mdTypeSpec        , TypeSpec         > TypeSpecMap        ; typedef TypeSpecMap        ::iterator TypeSpecMapIter        ;
    typedef std::map     < mdCustomAttribute , CustomAttribute  > CustomAttributeMap ; typedef CustomAttributeMap ::iterator CustomAttributeMapIter ;

    typedef std::map     < mdString          , std::wstring     > UserStringMap      ; typedef UserStringMap      ::iterator UserStringMapIter      ;

    typedef std::map     < mdManifestResource, ManifestResource > ManifestResourceMap; typedef ManifestResourceMap::iterator ManifestResourceMapIter;
    typedef std::multimap< mdToken           , ParsedResource   > ParsedResourceMap  ; typedef ParsedResourceMap  ::iterator ParsedResourceMapIter  ;

    //--//

    class Parser
    {
    public:
        typedef std::list<         FieldDef     > fieldDefList ; typedef fieldDefList ::iterator fieldDefListIter;
        typedef std::map< mdToken, fieldDefList > fieldDefMap  ; typedef fieldDefMap  ::iterator fieldDefMapIter;

        typedef std::list<         MethodDef    > methodDefList; typedef methodDefList::iterator methodDefListIter;
        typedef std::map< mdToken, MethodDef    > methodDefMap ; typedef methodDefMap ::iterator methodDefMapIter;

        //--//

        Collection*                      m_holder;
        std::wstring                     m_assemblyFile;

        std::wstring                     m_assemblyName;
        CLR_RECORD_VERSION               m_version;
        mdToken                          m_entryPointToken;
        mdAssembly                       m_tkAsm;

        AssemblyRefMap                   m_mapRef_Assembly;
        ModuleRefMap                     m_mapRef_Module;
        TypeRefMap                       m_mapRef_Type;
        MemberRefMap                     m_mapRef_Member;

        TypeDefMap                       m_mapDef_Type;
        FieldDefMap                      m_mapDef_Field;
        MethodDefMap                     m_mapDef_Method;
        InterfaceImplMap                 m_mapDef_Interface;

        CustomAttributeMap               m_mapDef_CustomAttribute;
        mdTokenSet                       m_setAttributes_Methods_NativeProfiler;
        mdTokenSet                       m_setAttributes_Methods_GloballySynchronized;
        mdTokenSet                       m_setAttributes_Types_PublishInApplicationDirectory;
        mdTokenSet                       m_setAttributes_Fields_NoReflection;

        TypeSpecMap                      m_mapSpec_Type;

        UserStringMap                    m_mapDef_String;
        ManifestResourceMap              m_mapDef_ManifestResource;

        //--//

        bool                             m_fVerboseMinimize;

        bool                             m_fNoByteCode;
        bool                             m_fNoAttributes;
        CLR_RT_StringSet                 m_setFilter_ExcludeClassByName;

        CLR_RT_StringSet                 m_resources;
        ISymUnmanagedReaderPtr           m_pSymReader;

    private:
        IMetaDataDispenserExPtr          m_pDisp;
        IMetaDataImportPtr               m_pImport;
        IMetaDataImport2Ptr              m_pImport2;
        IMetaDataAssemblyImportPtr       m_pAssemblyImport;
        PELoader                         m_pe;
        FILE*                            m_output;
        FILE*                            m_toclose;

        //--//

        HRESULT GetAssemblyDef     (                       );
        HRESULT GetAssemblyRef     ( mdAssemblyRef      ar );
        HRESULT GetModuleRef       ( mdModuleRef        mr );
        HRESULT GetTypeRef         ( mdTypeRef          tr );
        HRESULT GetMemberRef       ( mdMemberRef        mr );
        HRESULT GetTypeDef         ( mdTypeDef          td );
        HRESULT GetTypeField       ( mdFieldDef         fd );
        HRESULT GetTypeMethod      ( mdMethodDef        md );
        HRESULT GetTypeInterface   ( mdInterfaceImpl    ii );
        HRESULT GetCustomAttribute ( mdCustomAttribute  ca );
        HRESULT GetTypeSpec        ( mdTypeSpec         ts );
        HRESULT GetUserString      ( mdString           s  );
        HRESULT GetManifestResource( mdManifestResource mr );

        HRESULT EnumAssemblyRefs     (               );
        HRESULT EnumModuleRefs       (               );
        HRESULT EnumTypeRefs         (               );
        HRESULT EnumMemberRefs       ( TypeRef&   tr );
        HRESULT EnumTypeDefs         (               );
        HRESULT EnumTypeFields       ( TypeDef&   td );
        HRESULT EnumTypeMethods      ( TypeDef&   td );
        HRESULT EnumTypeInterfaces   ( TypeDef&   td );
        HRESULT EnumCustomAttributes (               );
        HRESULT EnumTypeSpecs        (               );
        HRESULT EnumUserStrings      (               );
        HRESULT EnumManifestResources(               );
        HRESULT EnumGenericParams    ( mdToken   tk  );

        HRESULT ParseResource       ( CustomAttribute& ca, CLR_UINT16 kind );

        HRESULT ParseByteCode       ( MethodDef& db );

        HRESULT CanIncludeMember    ( mdToken tk, mdToken     tm  );
        HRESULT BuildDependencyList ( mdToken tk, mdTokenSet& set );
        HRESULT IncludeAttributes   ( mdToken tk, mdTokenSet& set );

        //--//

        void Dump_SetDevice  ( LPCWSTR szFileName );
        void Dump_CloseDevice(                    );

        void Dump_PrintSigForType    ( TypeSignature    & sig );
        void Dump_PrintSigForMethod  ( MethodSignature  & sig );
        void Dump_PrintSigForLocalVar( LocalVarSignature& sig );
        void Dump_PrintSigForTypeSpec( TypeSpecSignature& sig );

        void Dump_EnumAssemblyRefs    (                  );
        void Dump_EnumModuleRefs      (                  );
        void Dump_EnumTypeRefs        (                  );
        void Dump_EnumTypeDefs        ( bool fNoByteCode );
        void Dump_EnumCustomAttributes(                  );
        void Dump_EnumUserStrings     (                  );

        void Dump_ShowDependencies( mdToken tk, mdTokenSet& set, mdTokenSet& setAdd );

        //--//

        int SizeFromElementType( CorElementType et );

        //
        bool                           m_fSwapEndian;

    public:
        Parser( Collection* holder );
        ~Parser();

        HRESULT Analyze( LPCWSTR szFileName );

        HRESULT RemoveUnused();

        HRESULT VerifyConsistency();

        void    DumpCompact( LPCWSTR szFileName                   );
        void    DumpSchema ( LPCWSTR szFileName, bool fNoByteCode );

        //--//

        bool    CheckIsTokenPresent ( mdToken     tk  );
        HRESULT CheckTokenPresence  ( mdToken     tk  );
        HRESULT CheckTokensPresence ( mdTokenSet& set );

        void TokenToString( mdToken tk, std::wstring& str );
        bool SetSwapEndian( bool State ) { m_fSwapEndian=State; }
        bool GetSwapEndian( void ) { return m_fSwapEndian; }
    };

    class Collection
    {
        friend class Parser;

        typedef std::map< std::wstring, std::wstring > LoadHintsMap ; typedef LoadHintsMap ::iterator LoadHintsMapIter;
        typedef std::map< std::wstring, Parser*      > AssembliesMap; typedef AssembliesMap::iterator AssembliesMapIter;

        //--//

        CLR_RT_StringSet m_setIgnoreAssemblies;
        LoadHintsMap     m_mapLoadHints;
        AssembliesMap    m_mapAssemblies;

        //--//

        HRESULT FromNameToFile( const std::wstring& name, std::wstring& file );

        bool FileExists( const std::wstring& assemblyName, const std::wstring& targetPath, std::wstring& filename );
        bool FileExists( const std::wstring& filename );


    public:
        Collection();
        ~Collection();

        void Clear( bool fAll );

        HRESULT IgnoreAssembly( LPCWSTR szAssemblyName                     );
        HRESULT LoadHints     ( LPCWSTR szAssemblyName, LPCWSTR szFileName );

        HRESULT CreateAssembly         (                     Parser*& pr );
        HRESULT CreateDependentAssembly( LPCWSTR szFileName, Parser*& pr );

        HRESULT ResolveAssemblyDef( Parser* pr, mdToken tk, Parser*& prDst                    );
        HRESULT ResolveTypeDef    ( Parser* pr, mdToken tk, Parser*& prDst, TypeDef*  & tdDst );
        HRESULT ResolveMethodDef  ( Parser* pr, mdToken tk, Parser*& prDst, MethodDef*& mdDst );
        HRESULT ResolveFieldDef   ( Parser* pr, mdToken tk, Parser*& prDst, FieldDef* & fdDst );

        bool IsAssemblyToken( Parser* pr, mdToken tk );
        bool IsTypeToken    ( Parser* pr, mdToken tk );
        bool IsMethodToken  ( Parser* pr, mdToken tk );
        bool IsFieldToken   ( Parser* pr, mdToken tk );
    };
};
