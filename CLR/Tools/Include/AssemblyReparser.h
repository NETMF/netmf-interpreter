////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#pragma once

#include <AssemblyParser.h>

#include <TinyCLR_Runtime.h>

//////////////////////////////////////////////////////

#define REPARSER_TABLE_ENUM_BEGIN(obj,src)                                                                                           \
    {                                                                                                                                \
        for(MetaData::Reparser::TokenToObjectIter it=obj->m_lookupTokenToObject.begin(); it!=obj->m_lookupTokenToObject.end(); it++) \
        {                                                                                                                            \
            if(CLR_TypeFromTk( (CLR_UINT32)it->first ) == src::GetTable())                                                           \
            {                                                                                                                        \
                src* rec = it->second.CastTo( src() );

#define REPARSER_TABLE_ENUM_END()                                                                                                    \
            }                                                                                                                        \
        }                                                                                                                            \
    }

//--//

//
// Forward declaration...
//
namespace WatchAssemblyBuilder
{
    class Linker;
}

//--//

namespace MetaData
{
    namespace Reparser
    {
        struct BaseTokenInner;
        struct BaseToken;
        struct BaseTokenPtr;

        struct TypeSignature;

        class Assembly;

        //--//--//--//

        struct BaseElement
        {
            Assembly*   m_holder;
            std::string m_displayString;

            BaseElement();

            bool operator==( BaseElement& be )                        ;
            bool operator!=( BaseElement& be ) { return !(*this == be); }

            //--//

            const std::string& GetDisplayString();

            //--//

        protected:
            virtual void BuildString() = 0;
        };

        struct BaseTokenInner : public BaseElement
        {
            mdToken m_tk;

            BaseTokenInner()
            {
                Init( NULL, CLR_EmptyToken );
            }

            void Init( Assembly* holder, mdToken tk )
            {
                m_holder = holder;
                m_tk     = tk;
            }

            //--//

            virtual CLR_TABLESENUM GetTableOfInstance() const = 0;
        };

        struct BaseTokenPtr
        {
            BaseTokenInner* m_ptr;

            //--//

            BaseTokenPtr()
            {
                m_ptr = NULL;
            }

            BaseTokenPtr( BaseToken* bt )
            {
                m_ptr = (BaseTokenInner*)bt;
            }

            BaseTokenPtr& operator=( BaseToken* bt )
            {
                m_ptr = (BaseTokenInner*)bt;

                return *this;
            }

            bool operator!() const { return m_ptr == NULL; }

            operator BaseToken*() const { return (BaseToken*)m_ptr; }

            BaseToken* operator->() const { return (BaseToken*)m_ptr; }

            template <class T> T* CastTo( const T& selector ) const
            {
                if(m_ptr && m_ptr->GetTableOfInstance() == T::GetTable()) return (T*)m_ptr;

                return NULL;
            }
        };

        typedef std::list<BaseTokenPtr>          BaseTokenPtrList;
        typedef BaseTokenPtrList::iterator       BaseTokenPtrIter;
        typedef BaseTokenPtrList::const_iterator BaseTokenPtrConstIter;

        struct BaseToken : public BaseTokenInner
        {
            BaseTokenPtr m_orig;
            BaseTokenPtr m_patched;
        };

        //--//--//--//

        struct TypeSignaturePtr
        {
            TypeSignature* m_ptr;

            //--//

            TypeSignaturePtr();

            TypeSignaturePtr           ( const TypeSignaturePtr& ots );
            TypeSignaturePtr& operator=( const TypeSignaturePtr& ots );

            ~TypeSignaturePtr();

            void Allocate();

            bool operator!() const { return m_ptr == NULL; }

            operator TypeSignature*() const { return (TypeSignature*)m_ptr; }

            TypeSignature* operator->() const { return m_ptr; }
        };

        struct TypeSignature : public BaseElement
        {
            CLR_DataType     m_opt;
            BaseTokenPtr     m_token;
            TypeSignaturePtr m_sub;

            //--//

            TypeSignature()
            {
                m_opt = DATATYPE_FIRST_INVALID;
            }

            //--//

            void Parse( Assembly* holder, CLR_RT_Assembly* assm, CLR_PMETADATA& pSigBlob );

        private:
            void ParseToken  ( CLR_RT_Assembly* assm, CLR_PMETADATA& pSigBlob );
            void ParseSubType( CLR_RT_Assembly* assm, CLR_PMETADATA& pSigBlob );

        protected:
            virtual void BuildString();
        };

        typedef std::list<TypeSignature>          TypeSignatureList;
        typedef TypeSignatureList::iterator       TypeSignatureIter;
        typedef TypeSignatureList::const_iterator TypeSignatureConstIter;


        struct MethodSignature : public BaseElement
        {
            BYTE              m_flags;
            TypeSignature     m_retValue;
            TypeSignatureList m_lstParams;

            //--//

            MethodSignature()
            {
                m_flags = 0;
            }

            //--//

            void Parse( Assembly* holder, CLR_RT_Assembly* assm, CLR_PMETADATA& pSigBlob );

            bool operator==( MethodSignature& sig )                         ;
            bool operator!=( MethodSignature& sig ) { return !(*this == sig); }

        protected:
            virtual void BuildString();
        };

        struct LocalVarSignature : public BaseElement
        {
            TypeSignatureList m_lstVars;

            //--//

            void Parse( Assembly* holder, CLR_RT_Assembly* assm, CLR_PMETADATA& pSigBlob, CLR_UINT32 num );

            bool operator==( LocalVarSignature& sig )                         ;
            bool operator!=( LocalVarSignature& sig ) { return !(*this == sig); }

        protected:
            virtual void BuildString();
        };

        //--//--//--//

        struct AssemblyRef : public BaseToken
        {
            std::string        m_name;
            CLR_RECORD_VERSION m_version;

            //--//

            void Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_ASSEMBLYREF* src );

            virtual CLR_TABLESENUM GetTableOfInstance() const { return GetTable(); }

            static CLR_TABLESENUM GetTable() { return TBL_AssemblyRef; }

        protected:
            virtual void BuildString();
        };

        //--//

        struct TypeRef : public BaseToken
        {
            std::string  m_name;
            std::string  m_nameSpace;

            BaseTokenPtr m_scope;

            //--//

            void Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_TYPEREF* src );

            virtual CLR_TABLESENUM GetTableOfInstance() const { return GetTable(); }

            static CLR_TABLESENUM GetTable() { return TBL_TypeRef; }

        protected:
            virtual void BuildString();
        };

        //--//

        struct FieldRef : public BaseToken
        {
            std::string   m_name;
            BaseTokenPtr  m_container;

            TypeSignature m_sig;

            //--//

            void Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_FIELDREF* src );

            virtual CLR_TABLESENUM GetTableOfInstance() const { return GetTable(); }

            static CLR_TABLESENUM GetTable() { return TBL_FieldRef; }

        protected:
            virtual void BuildString();
        };

        //--//

        struct MethodRef : public BaseToken
        {
            std::string     m_name;
            BaseTokenPtr    m_container;

            MethodSignature m_sig;

            //--//

            void Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_METHODREF* src );

            virtual CLR_TABLESENUM GetTableOfInstance() const { return GetTable(); }

            static CLR_TABLESENUM GetTable() { return TBL_MethodRef; }

        protected:
            virtual void BuildString();
        };

        //--//

        struct TypeDef : public BaseToken
        {
            std::string         m_name;
            std::string         m_nameSpace;

            BaseTokenPtr        m_extends;
            BaseTokenPtr        m_enclosingType;

            BaseTokenPtrList    m_interfaces;

            BaseTokenPtrList    m_methods_Virtual;
            BaseTokenPtrList    m_methods_Instance;
            BaseTokenPtrList    m_methods_Static;
            CLR_UINT8           m_dataType;

            BaseTokenPtrList    m_fields_Static;
            BaseTokenPtrList    m_fields_Instance;

            CLR_UINT16          m_flags;

            //--//

            void Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_TYPEDEF* src );

            bool IsCompatible( TypeDef* ptr );

            virtual CLR_TABLESENUM GetTableOfInstance() const { return GetTable(); }

            static CLR_TABLESENUM GetTable() { return TBL_TypeDef; }

        protected:
            virtual void BuildString();

        private:
            void ParseMethod( BaseTokenPtrList& lst, CLR_RT_Assembly* assm, CLR_IDX pos );
            void ParseField ( BaseTokenPtrList& lst, CLR_RT_Assembly* assm, CLR_IDX pos );
        };

        //--//

        struct FieldDef : public BaseToken
        {
            BaseTokenPtr  m_container;

            std::string   m_name;
            CLR_UINT16    m_flags;

            TypeSignature m_sig;

            CLR_RT_Buffer m_defaultValue;

            //--//

            void Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_FIELDDEF* src );

            bool IsCompatible( FieldDef* ptr );

            virtual CLR_TABLESENUM GetTableOfInstance() const { return GetTable(); }

            static CLR_TABLESENUM GetTable() { return TBL_FieldDef; }

        protected:
            virtual void BuildString();
        };

        //--//

        struct MethodDef : public BaseToken
        {
            struct Eh
            {
                CLR_UINT32   m_mode;
                BaseTokenPtr m_classToken;
                CLR_OFFSET   m_tryStart;
                CLR_OFFSET   m_tryEnd;
                CLR_OFFSET   m_handlerStart;
                CLR_OFFSET   m_handlerEnd;
            };

            typedef std::list<Eh>    EhList;
            typedef EhList::iterator EhIter;

            typedef std::map<size_t,BaseTokenPtr> TokenMap;
            typedef TokenMap::iterator            TokenIter;

            //--//

            BaseTokenPtr      m_container;

            std::string       m_name;
            CLR_RT_Buffer     m_byteCode;
            TokenMap          m_tokens;
            EhList            m_eh;

            CLR_UINT32        m_flags;

            CLR_UINT8         m_retVal;
            CLR_UINT8         m_numArgs;
            CLR_UINT8         m_numLocals;
            CLR_UINT8         m_lengthEvalStack;

            LocalVarSignature m_locals;
            MethodSignature   m_sig;

            //--//

            void Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_METHODDEF* src );

            bool IsCompatible( MethodDef* ptr );

            virtual CLR_TABLESENUM GetTableOfInstance() const { return GetTable(); }

            static CLR_TABLESENUM GetTable() { return TBL_MethodDef; }

        protected:
            virtual void BuildString();
        };

        //--//

        struct Attribute : public BaseToken
        {
            struct Reader
            {
                CLR_RT_Assembly* m_assm;
                CLR_PMETADATA    m_sig;

                Reader( CLR_RT_Assembly* assm, CLR_PMETADATA sig );

                void Read( CLR_RT_Buffer&        val, size_t len );
                void Read( std::string&          val             );
                void Read( CorSerializationType& val             );
                void Read( CLR_UINT16&           val             );
            };

            struct Name
            {
                CorSerializationType m_opt;
                std::string          m_text;

                Name()
                {
                    m_opt = (CorSerializationType)ELEMENT_TYPE_VOID;
                }

                void Parse( Reader& reader );

                bool operator<( const Name& r ) const
                {
                    if(m_opt < r.m_opt) return true;
                    if(m_opt > r.m_opt) return false;

                    return m_text < r.m_text;
                }

                bool operator==( const Name& r ) const
                {
                    return m_opt == r.m_opt && m_text == r.m_text;
                }

                bool operator!=( const Name& r ) const
                {
                    return !(*this == r);
                }
            };

            struct Value
            {
                CorSerializationType m_opt;
                CLR_RT_Buffer        m_numeric;
                std::string          m_text;

                Value()
                {
                    m_opt = (CorSerializationType)ELEMENT_TYPE_VOID;
                }

                void Parse( Reader& reader );

                bool operator==( const Value& v ) const
                {
                    if(m_opt     != v.m_opt    ) return false;
                    if(m_numeric != v.m_numeric) return false;
                    if(m_text    != v.m_text   ) return false;

                    return true;
                }

                bool operator!=( const Value& v ) const
                {
                    return !(*this == v);
                }
            };

            typedef std::list<      Value > ValueList; typedef ValueList::iterator ValueListIter;
            typedef std::map< Name, Value > ValueMap ; typedef ValueMap ::iterator ValueMapIter;

            //--//

            BaseTokenPtr m_owner;
            BaseTokenPtr m_constructor;
            ValueList    m_valuesFixed;
            ValueMap     m_valuesVariable;

            //--//

            void Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_ATTRIBUTE* src );

            bool IsCompatible( Attribute* ptr );

            virtual CLR_TABLESENUM GetTableOfInstance() const { return GetTable(); }

            static CLR_TABLESENUM GetTable() { return TBL_Attributes; }

        protected:
            virtual void BuildString();
        };

        //--//

        struct TypeSpec : public BaseToken
        {
            TypeSignature m_sig;

            //--//

            void Parse( BaseToken* container, CLR_RT_Assembly* assm, const CLR_RECORD_TYPESPEC* src );

            virtual CLR_TABLESENUM GetTableOfInstance() const { return GetTable(); }

            static CLR_TABLESENUM GetTable() { return TBL_TypeSpec; }

        protected:
            virtual void BuildString();
        };

        //--//

        struct String : public BaseToken
        {
            std::string m_value;

            //--//

            void Parse( BaseToken* container, CLR_RT_Assembly* assm, LPCSTR src );

            bool IsCompatible( String* ptr );

            virtual CLR_TABLESENUM GetTableOfInstance() const { return GetTable(); }

            static CLR_TABLESENUM GetTable() { return TBL_Strings; }

        protected:
            virtual void BuildString();
        };

        //--//

        typedef std::map<mdToken,BaseTokenPtr>     TokenToObjectMap;
        typedef TokenToObjectMap::iterator         TokenToObjectIter;
        typedef TokenToObjectMap::const_iterator   TokenToObjectConstIter;

        typedef std::map<std::string,BaseTokenPtr> StringToObjectMap;
        typedef StringToObjectMap::iterator        StringToObjectIter;
        typedef StringToObjectMap::const_iterator  StringToObjectConstIter;

        typedef std::set< std::string >            StringSet;
        typedef StringSet::iterator                StringSetIter;
        typedef StringSet::const_iterator          StringSetConstIter;

        //--//

        class Assembly : public AssemblyRef
        {
            friend class WatchAssemblyBuilder::Linker;

            std::wstring      m_assemblyFile;

            TokenToObjectMap  m_lookupTokenToObject;
            StringToObjectMap m_lookupStringToObject;

            StringToObjectMap m_lookupStringToObjectToCreate;
            StringToObjectMap m_lookupStringToObjectToPatch;

            BaseTokenPtrList  m_objectsToProcess;

            Assembly*         m_assemblyOrig;
            Assembly*         m_assemblyPatched;

            //--//

            typedef HRESULT (Assembly::*CompareFtn)( Assembly* other, StringSet* equal, StringSet* changed, StringSet* missing );

            //--//

        public:
            HRESULT Load( LPCWSTR szFile );

            HRESULT CreateDiff( Assembly* orig, Assembly* patched, bool fForceAssemblyRef );

            static void BuildString( std::string& value, LPCSTR szName, const CLR_RECORD_VERSION& ver );

            static bool CompareToken  ( Assembly* orig, mdToken tkOrig , Assembly* patched, mdToken tkPatched  );
            static bool CompareObject ( const BaseTokenPtr&     ptrOrig, const BaseTokenPtr&        ptrPatched );
            static bool CompareObjects( const BaseTokenPtrList& lstOrig, const BaseTokenPtrList&    lstPatched );

            void Dump();

            //--//

        public:
            BaseToken* Parse( CLR_RT_Assembly* assm, mdToken tk                       ) { return Parse( assm, tk, NULL ); }
            BaseToken* Parse( CLR_RT_Assembly* assm, mdToken tk, BaseToken* container );

            //--//

            template <class T> T* FindObject( mdToken tk, const T& selector ) const
            {
                return FindObject( tk ).CastTo( selector );
            }

            const BaseTokenPtr& FindObject( mdToken tk ) const;

            //--//

            template <class T> T* FindObject( const std::string& value, const T& selector  ) const
            {
                return FindObject( value ).CastTo( selector );
            }

            const BaseTokenPtr& FindObject( const std::string& value ) const;

            //--//

            void DumpError( LPCSTR fmt, StringSet& set ) const;

            //--//

        private:
            struct DiffData
            {
                StringSet m_setEqual;
                StringSet m_setChanged;
                StringSet m_setDeleted;
                StringSet m_setAdded;

                HRESULT Compute( CompareFtn ftn, Assembly* orig, Assembly* patched );
            };

            HRESULT Compare_TypeDef  ( Assembly* other, StringSet* equal, StringSet* changed, StringSet* missing );
            HRESULT Compare_FieldDef ( Assembly* other, StringSet* equal, StringSet* changed, StringSet* missing );
            HRESULT Compare_MethodDef( Assembly* other, StringSet* equal, StringSet* changed, StringSet* missing );
            HRESULT Compare_Attribute( Assembly* other, StringSet* equal, StringSet* changed, StringSet* missing );

            //--//

            BaseToken* AddToCreate( BaseToken*              ptr, BaseToken* container );
            void       AddToCreate( const BaseTokenPtrList& lst, BaseToken* container );

            TypeDef*   AddToPatch( TypeDef*   ptr );
            MethodDef* AddToPatch( MethodDef* ptr );

            bool IsAPatch     ( BaseToken* ptr ) const;
            bool IsAFullObject( BaseToken* ptr ) const;

            void Resolve( Assembly* orig, Assembly* patched );

            //--//

            BaseToken* Clone( BaseToken* ptr );

            void Clone( BaseTokenPtrList & lstDst, const BaseTokenPtrList & lstSrc );
            void Clone( TypeSignatureList& lstDst, const TypeSignatureList& lstSrc );

            void Clone( TypeSignature    & sigDst, const TypeSignature    & sigSrc );
            void Clone( MethodSignature  & sigDst, const MethodSignature  & sigSrc );
            void Clone( LocalVarSignature& sigDst, const LocalVarSignature& sigSrc );
        };
    };
};
