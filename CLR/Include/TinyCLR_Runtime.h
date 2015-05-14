////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_RUNTIME_H_
#define _TINYCLR_RUNTIME_H_

////////////////////////////////////////////////////////////////////////////////////////////////////

#include <TinyCLR_Types.h>
#include <TinySupport.h>
#include <TinyCLR_Interop.h>
#include <TinyCLR_ErrorCodes.h>

struct CLR_RADIAN
{
    short cos;
    short sin;
};

extern const CLR_RADIAN c_CLR_radians[];

#if defined(PLATFORM_WINCE)
#include <msxml2.h>         //  Required to import WinCE / XMLDOM stuff....
#endif

//--//

#if defined(PLATFORM_WINDOWS) || defined(PLATFORM_WINCE)

#include <set>
#include <map>
#include <list>
#include <string>
#include <vector>
#include <comutil.h>
#include <comip.h>

typedef std::set< std::wstring >             CLR_RT_StringSet;
typedef CLR_RT_StringSet::iterator           CLR_RT_StringSetIter;

typedef std::map< std::string,int >          CLR_RT_StringMap;
typedef CLR_RT_StringMap::iterator           CLR_RT_StringMapIter;

typedef std::vector< std::wstring >          CLR_RT_StringVector;
typedef CLR_RT_StringVector::iterator        CLR_RT_StringVectorIter;

typedef std::map< std::wstring, CLR_UINT32 > CLR_RT_SymbolToAddressMap;
typedef CLR_RT_SymbolToAddressMap::iterator  CLR_RT_SymbolToAddressMapIter;

typedef std::map< CLR_UINT32, std::wstring > CLR_RT_AddressToSymbolMap;
typedef CLR_RT_AddressToSymbolMap::iterator  CLR_RT_AddressToSymbolMapIter;

#pragma pack(push, TINYCLR_RUNTIME_H, 4)

//--//

#define _COM_SMRT_PTR(i)     typedef _com_ptr_t<_com_IIID<i, &__uuidof(i)>> i ## Ptr
#define _COM_SMRT_PTR_2(i,u) typedef _com_ptr_t<_com_IIID<i, &u          >> i ## Ptr


_COM_SMRT_PTR(IXMLDOMDocument);
_COM_SMRT_PTR(IXMLDOMNode);

class CLR_XmlUtil
{
    IXMLDOMDocumentPtr m_xddDoc;
    IXMLDOMNodePtr m_xdnRoot;
    HANDLE                   m_hEvent;    // Used to abort a download.
    DWORD                    m_dwTimeout; // Used to limit a download.

    void Init ();
    void Clean();

    HRESULT LoadPost( /*[in] */ LPCWSTR szRootTag ,
                      /*[out]*/ bool&   fLoaded   ,
                      /*[out]*/ bool*   fFound    );

    HRESULT CreateParser();

public:
    CLR_XmlUtil( /*[in]*/ const CLR_XmlUtil& xml                                               );
    CLR_XmlUtil( /*[in]*/ IXMLDOMDocument*   xddDoc        , /*[in]*/ LPCWSTR szRootTag = NULL );
    CLR_XmlUtil( /*[in]*/ IXMLDOMNode*       xdnRoot = NULL, /*[in]*/ LPCWSTR szRootTag = NULL );

    ~CLR_XmlUtil();


    CLR_XmlUtil& operator=( /*[in]*/ const CLR_XmlUtil& xml     );
    CLR_XmlUtil& operator=( /*[in]*/ IXMLDOMNode*       xdnRoot );

    HRESULT DumpError();

    HRESULT New         (                                /*[in]*/ IXMLDOMNode* xdnRoot  , /*[in] */ BOOL    fDeep      = false                     );
    HRESULT New         (                                /*[in]*/ LPCWSTR      szRootTag, /*[in] */ LPCWSTR szEncoding = L"utf-8" /*L"unicode"*/   );
    HRESULT Load        ( /*[in ]*/ LPCWSTR    szFile  , /*[in]*/ LPCWSTR      szRootTag, /*[out]*/ bool&   fLoaded, /*[out]*/ bool* fFound = NULL );
    HRESULT LoadAsStream( /*[in ]*/ IUnknown*  pStream , /*[in]*/ LPCWSTR      szRootTag, /*[out]*/ bool&   fLoaded, /*[out]*/ bool* fFound = NULL );
    HRESULT LoadAsString( /*[in ]*/ BSTR       bstrData, /*[in]*/ LPCWSTR      szRootTag, /*[out]*/ bool&   fLoaded, /*[out]*/ bool* fFound = NULL );
    HRESULT Save        ( /*[in ]*/ LPCWSTR    szFile                                                                                              );
    HRESULT SaveAsStream( /*[out]*/ IUnknown* *ppStream                                                                                            );
    HRESULT SaveAsString( /*[out]*/ BSTR      *pbstrData                                                                                           );

    HRESULT SetTimeout( /*[in]*/ DWORD dwTimeout );
    HRESULT Abort     (                          );

    HRESULT SetVersionAndEncoding( /*[in]*/ LPCWSTR szVersion, /*[in]*/ LPCWSTR szEncoding );

    HRESULT GetDocument    (                         /*[out]*/ IXMLDOMDocument*  * pVal                                        ) const;
    HRESULT GetRoot        (                         /*[out]*/ IXMLDOMNode*      * pVal                                        ) const;
    HRESULT GetNodes       ( /*[in]*/ LPCWSTR szTag, /*[out]*/ IXMLDOMNodeList*  * pVal                                        ) const;
    HRESULT GetNode        ( /*[in]*/ LPCWSTR szTag, /*[out]*/ IXMLDOMNode*      * pVal                                        ) const;
    HRESULT CreateNode     ( /*[in]*/ LPCWSTR szTag, /*[out]*/ IXMLDOMNode*      * pVal, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );

    HRESULT GetAttribute   ( /*[in]*/ LPCWSTR szTag, /*[in]*/ LPCWSTR szAttr, /*[out]*/ IXMLDOMAttribute*  *   pValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT GetAttribute   ( /*[in]*/ LPCWSTR szTag, /*[in]*/ LPCWSTR szAttr, /*[out]*/ _bstr_t&           bstrValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT GetAttribute   ( /*[in]*/ LPCWSTR szTag, /*[in]*/ LPCWSTR szAttr, /*[out]*/ std::wstring&         szValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT GetAttribute   ( /*[in]*/ LPCWSTR szTag, /*[in]*/ LPCWSTR szAttr, /*[out]*/ LONG&                  lValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT GetValue       ( /*[in]*/ LPCWSTR szTag,                          /*[out]*/ _variant_t&           vValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT GetValue       ( /*[in]*/ LPCWSTR szTag,                          /*[out]*/ _bstr_t&           bstrValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT GetValue       ( /*[in]*/ LPCWSTR szTag,                          /*[out]*/ std::wstring&         szValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );

    HRESULT ModifyAttribute( /*[in]*/ LPCWSTR szTag, /*[in]*/ LPCWSTR szAttr, /*[in] */ const _bstr_t&     bstrValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT ModifyAttribute( /*[in]*/ LPCWSTR szTag, /*[in]*/ LPCWSTR szAttr, /*[in] */ const std::wstring&   szValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT ModifyAttribute( /*[in]*/ LPCWSTR szTag, /*[in]*/ LPCWSTR szAttr, /*[in] */ LPCWSTR               szValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT ModifyAttribute( /*[in]*/ LPCWSTR szTag, /*[in]*/ LPCWSTR szAttr, /*[in] */ LONG                   lValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT ModifyValue    ( /*[in]*/ LPCWSTR szTag,                          /*[in] */ const _variant_t&     vValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT ModifyValue    ( /*[in]*/ LPCWSTR szTag,                          /*[in] */ const _bstr_t&     bstrValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT ModifyValue    ( /*[in]*/ LPCWSTR szTag,                          /*[in] */ const std::wstring&   szValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );

    HRESULT PutAttribute   ( /*[in]*/ LPCWSTR szTag, /*[in]*/ LPCWSTR szAttr, /*[in] */ IXMLDOMAttribute*  *   pValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT PutAttribute   ( /*[in]*/ LPCWSTR szTag, /*[in]*/ LPCWSTR szAttr, /*[in] */ const _bstr_t&     bstrValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT PutAttribute   ( /*[in]*/ LPCWSTR szTag, /*[in]*/ LPCWSTR szAttr, /*[in] */ const std::wstring&   szValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT PutAttribute   ( /*[in]*/ LPCWSTR szTag, /*[in]*/ LPCWSTR szAttr, /*[in] */ LPCWSTR               szValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT PutAttribute   ( /*[in]*/ LPCWSTR szTag, /*[in]*/ LPCWSTR szAttr, /*[in] */ LONG                   lValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT PutValue       ( /*[in]*/ LPCWSTR szTag,                          /*[in] */ const _variant_t&     vValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT PutValue       ( /*[in]*/ LPCWSTR szTag,                          /*[in] */ const _bstr_t&     bstrValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT PutValue       ( /*[in]*/ LPCWSTR szTag,                          /*[in] */ const std::wstring&   szValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT PutValue       ( /*[in]*/ LPCWSTR szTag,                          /*[in] */ LPCWSTR               szValue, /*[out]*/ bool& fFound, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );

    HRESULT RemoveAttribute( /*[in]*/ LPCWSTR szTag, /*[in]*/ LPCWSTR szAttr, /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT RemoveValue    ( /*[in]*/ LPCWSTR szTag,                          /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
    HRESULT RemoveNode     ( /*[in]*/ LPCWSTR szTag,                          /*[in]*/ IXMLDOMNode* pxdnNode = NULL );
};

//--//


typedef std::vector< CLR_UINT8 > CLR_RT_Buffer;

struct CLR_RT_FileStore
{
    static HRESULT LoadFile( LPCWSTR szFile,       CLR_RT_Buffer& vec          );
    static HRESULT SaveFile( LPCWSTR szFile, const CLR_RT_Buffer& vec          );
    static HRESULT SaveFile( LPCWSTR szFile, const CLR_UINT8* buf, size_t size );

    static HRESULT ExtractTokensFromFile( LPCWSTR szFileName, CLR_RT_StringVector& vec, LPCWSTR separators = L" \t", bool fNoComments = true );

    static void ExtractTokens          ( const CLR_RT_Buffer& buf   , CLR_RT_StringVector& vec, LPCWSTR separators = L" \t", bool fNoComments = true );
    static void ExtractTokensFromBuffer( LPWSTR               szLine, CLR_RT_StringVector& vec, LPCWSTR separators = L" \t", bool fNoComments = true );
    static void ExtractTokensFromString( LPCWSTR              szLine, CLR_RT_StringVector& vec, LPCWSTR separators = L" \t"                          );
};

#endif


////////////////////////////////////////////////////////////////////////////////////////////////////

#define MAX(a,b)   (a > b      ? a : b)
#define MIN(a,b)   (a < b      ? a : b)
#define ABS(x)     (x > 0      ? x : (-x))
#define FLOOR32(x) ((CLR_INT32)x)
#define FRAC(x)    (x - FLOOR32(x))

////////////////////////////////////////////////////////////////////////////////////////////////////


struct CLR_RT_HeapBlock;
struct CLR_RT_HeapBlock_Node;

struct CLR_RT_HeapBlock_WeakReference;
struct CLR_RT_HeapBlock_String;
struct CLR_RT_HeapBlock_Array;
struct CLR_RT_HeapBlock_Delegate;
struct CLR_RT_HeapBlock_Delegate_List;
struct CLR_RT_HeapBlock_BinaryBlob;

struct CLR_RT_HeapBlock_Button;
struct CLR_RT_HeapBlock_Lock;
struct CLR_RT_HeapBlock_LockRequest;
struct CLR_RT_HeapBlock_Timer;
struct CLR_RT_HeapBlock_WaitForObject;
struct CLR_RT_HeapBlock_Finalizer;
struct CLR_RT_HeapBlock_MemoryStream;

struct CLR_RT_HeapCluster;
struct CLR_RT_GarbageCollector;

struct CLR_RT_DblLinkedList;

#if defined(TINYCLR_APPDOMAINS)
struct CLR_RT_AppDomain;
struct CLR_RT_AppDomainAssembly;
#endif //TINYCLR_APPDOMAINS

struct CLR_RT_Assembly;
struct CLR_RT_TypeSystem;
struct CLR_RT_TypeDescriptor;

struct CLR_RT_Assembly_Instance;
struct CLR_RT_TypeSpec_Instance;
struct CLR_RT_TypeDef_Instance;
struct CLR_RT_MethodDef_Instance;
struct CLR_RT_FieldDef_Instance;

struct CLR_RT_StackFrame;
struct CLR_RT_SubThread;
struct CLR_RT_Thread;
struct CLR_RT_ExecutionEngine;

struct CLR_RT_ExecutionEngine_PerfCounters;

struct CLR_HW_Hardware;

typedef void (*CLR_RT_MarkingHandler   )( CLR_RT_HeapBlock_BinaryBlob* ptr );
typedef void (*CLR_RT_RelocationHandler)( CLR_RT_HeapBlock_BinaryBlob* ptr );
typedef void (*CLR_RT_HardwareHandler  )();


////////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_PROFILE_HANDLER)

struct CLR_PROF_CounterSimple;
struct CLR_PROF_Counter;
struct CLR_PROF_Counter_Value;
struct CLR_PROF_Counter_Value2;
struct CLR_PROF_Handler;

#if defined(TINYCLR_PROFILE_NEW_CALLS)

struct CLR_PROF_CounterCallChain
{
    CLR_UINT64         m_time_exclusive;
    CLR_RT_StackFrame* m_owningStackFrame;
    CLR_PROF_Handler*  m_owningHandler;


    //--//

    void* Prepare (                CLR_PROF_Handler* handler );
    void  Complete( CLR_UINT64& t, CLR_PROF_Handler* handler );

    void Enter( CLR_RT_StackFrame* stack );
    void Leave(                          );

    //--//

    PROHIBIT_COPY_CONSTRUCTORS(CLR_PROF_CounterCallChain);
};
#endif

//--//

struct CLR_PROF_Handler
{
    static const int c_Mode_Ignore    = 0;
    static const int c_Mode_Plain     = 1;
    static const int c_Mode_Simple    = 2;
#if defined(TINYCLR_PROFILE_NEW_CALLS)
    static const int c_Mode_CallChain = 3;
#endif

    static          bool              s_initialized;
    static          CLR_PROF_Handler* s_current;
    static volatile CLR_UINT64        s_time_overhead;
    static volatile CLR_UINT64        s_time_freeze;
    static volatile CLR_UINT64        s_time_adjusted;

    int                               m_target_Mode;
    void*                             m_target;

    CLR_PROF_Handler*                 m_containing;
    CLR_UINT64                        m_time_correction;
    CLR_UINT64                        m_time_start;

    //--//

    CLR_PROF_Handler(                                   ) { Constructor(        ); }

#if defined(TINYCLR_PROFILE_NEW_CALLS)
    CLR_PROF_Handler( CLR_PROF_CounterCallChain& target ) { Constructor( target ); }
#endif

    ~CLR_PROF_Handler() { Destructor(); }

    //--//

    static void       Calibrate    (             );
    static void       SuspendTime  (             );
    static CLR_UINT64 GetFrozenTime(             );
    static CLR_UINT64 ResumeTime   (             );
    static CLR_UINT64 ResumeTime   ( CLR_INT64 t );

    //--//

private:
    void Constructor(                                   );
#if defined(TINYCLR_PROFILE_NEW_CALLS)
    void Constructor( CLR_PROF_CounterCallChain& target );
#endif

    void Destructor();

    void Init( void* target );

    PROHIBIT_COPY_CONSTRUCTORS2(CLR_PROF_Handler);
};

//--//

#define CLR_PROF_HANDLER_SUSPEND_TIME() CLR_PROF_Handler::SuspendTime()
#define CLR_PROF_HANDLER_RESUME_TIME()  CLR_PROF_Handler::ResumeTime()

#else

#define CLR_PROF_HANDLER_SUSPEND_TIME()
#define CLR_PROF_HANDLER_RESUME_TIME()

#endif

#if defined(TINYCLR_PROFILE_NEW_CALLS)

#define CLR_PROF_HANDLER_CALLCHAIN_VOID(v) CLR_PROF_Handler v
#define CLR_PROF_HANDLER_CALLCHAIN(v,t)    CLR_PROF_Handler v( t )

#else

#define CLR_PROF_HANDLER_CALLCHAIN_VOID(v)
#define CLR_PROF_HANDLER_CALLCHAIN(v,t)

#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

struct CLR_RT_MemoryRange
{
    UINT8* m_location;
    UINT32 m_size;

    bool LimitToRange( CLR_RT_MemoryRange& filtered, UINT8* address, UINT32 length ) const;
};

extern CLR_RT_MemoryRange s_CLR_RT_Heap;

//--//

struct CLR_RT_Memory
{
    static void ZeroFill( void* buf, size_t len ) { memset( buf, 0, len ); }

    //--//

    static void  Reset             (                                  );
    static void* SubtractFromSystem( size_t len                       );
    static void  Release           ( void*  ptr                       );
    static void* Allocate          ( size_t len, CLR_UINT32 flags = 0 );
    static void* ReAllocate        ( void* ptr, size_t len            );
    static void* Allocate_And_Erase( size_t len, CLR_UINT32 flags = 0 );
};

//--//

struct CLR_RT_Random
{
private:
    int m_next;

public:
    void Initialize();
    void Initialize( int seed );

    int    Next();

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
    double NextDouble();
#else
    CLR_INT64 NextDouble();
#endif
    void   NextBytes( BYTE* buffer, UINT32 count );
};

//--//

extern void CLR_RT_GetVersion( UINT16* pMajor, UINT16* pMinor, UINT16* pBuild, UINT16* pRevision );

#define TINYCLR_CLEAR(ref) CLR_RT_Memory::ZeroFill( &ref, sizeof(ref) )

//--//

static const int c_CLR_RT_Trace_None      = 0;
static const int c_CLR_RT_Trace_Info      = 1;
static const int c_CLR_RT_Trace_Verbose   = 2;
static const int c_CLR_RT_Trace_Annoying  = 3;
static const int c_CLR_RT_Trace_Obnoxious = 4;

extern int  s_CLR_RT_fTrace_Errors;
extern int  s_CLR_RT_fTrace_Exceptions;
extern int  s_CLR_RT_fTrace_Instructions;
extern int  s_CLR_RT_fTrace_Memory;
extern int  s_CLR_RT_fTrace_MemoryStats;
extern int  s_CLR_RT_fTrace_StopOnFAILED;
extern int  s_CLR_RT_fTrace_GC;
extern int  s_CLR_RT_fTrace_GC_Depth;
extern int  s_CLR_RT_fTrace_SimulateSpeed;
extern int  s_CLR_RT_fTrace_AssemblyOverhead;

extern bool s_CLR_RT_fJitter_Enabled;
extern int  s_CLR_RT_fJitter_Trace_Statistics;
extern int  s_CLR_RT_fJitter_Trace_Compile;
extern int  s_CLR_RT_fJitter_Trace_Invoke;
extern int  s_CLR_RT_fJitter_Trace_Execution;

#if defined(PLATFORM_WINDOWS)
extern int          s_CLR_RT_fTrace_ARM_Execution;

extern int          s_CLR_RT_fTrace_RedirectLinesPerFile;
extern std::wstring s_CLR_RT_fTrace_RedirectOutput;
extern std::wstring s_CLR_RT_fTrace_RedirectCallChain;

extern std::wstring s_CLR_RT_fTrace_HeapDump_FilePrefix;
extern bool         s_CLR_RT_fTrace_HeapDump_IncludeCreators;

extern bool         s_CLR_RT_fTimeWarp;

#endif

static const int MAXHOSTNAMELEN = 256;  // Typical of many sockets implementations, incl. Windows
static const int MAXTYPENAMELEN = 256;  // Including terminating null byte. Enforced in MetadataProcessor. The standard imposes no limit, but we necessarily do.

////////////////////////////////////////////////////////////////////////////////////////////////////

#define TINYCLR_INDEX_IS_VALID(idx)   ((idx).m_data != 0)
#define TINYCLR_INDEX_IS_INVALID(idx) ((idx).m_data == 0)

//
// IMPORTANT: THE ASSEMBLY IDX IN ALL THE CLR_RT_*_Index STRUCTURES SHOULD ALWAYS BE ENCODED THE SAME WAY!!!
//
// For details, go to "bool CLR_RT_GarbageCollector::ComputeReachabilityGraphForMultipleBlocks( CLR_RT_HeapBlock* lstExt, CLR_UINT32 numExt )"
//

struct CLR_RT_Assembly_Index
{
    CLR_UINT32 m_data;

    //--//

    void Clear()
    {
        m_data = 0;
    }

    void Set( CLR_UINT32 idxAssm )
    {
        m_data = idxAssm << 16;
    }

    //--//

    CLR_IDX Assembly() const { return (CLR_IDX)(m_data >> 16); }
};

struct CLR_RT_TypeSpec_Index
{
    CLR_UINT32 m_data;

    //--//

    void Clear()
    {
        m_data = 0;
    }

    void Set( CLR_UINT32 idxAssm, CLR_UINT32 idxType )
    {
        m_data = idxAssm << 16 | idxType;
    }

    //--//

    CLR_IDX Assembly() const { return (CLR_IDX)(m_data >> 16); }
    CLR_IDX TypeSpec() const { return (CLR_IDX)(m_data      ); }
};

struct CLR_RT_TypeDef_Index
{
    CLR_UINT32 m_data;

    //--//

    void Clear()
    {
        m_data = 0;
    }

    void Set( CLR_UINT32 idxAssm, CLR_UINT32 idxType )
    {
        m_data = idxAssm << 16 | idxType;
    }

    //--//

    CLR_IDX Assembly() const { return (CLR_IDX)(m_data >> 16); }
    CLR_IDX Type    () const { return (CLR_IDX)(m_data      ); }
};

struct CLR_RT_FieldDef_Index
{
    CLR_UINT32 m_data;

    //--//

    void Clear()
    {
        m_data = 0;
    }

    void Set( CLR_UINT32 idxAssm, CLR_UINT32 idxField )
    {
        m_data = idxAssm << 16 | idxField;
    }

    //--//

    CLR_IDX Assembly() const { return (CLR_IDX)(m_data >> 16); }
    CLR_IDX Field   () const { return (CLR_IDX)(m_data      ); }
};

struct CLR_RT_MethodDef_Index
{
    CLR_UINT32 m_data;

    //--//

    void Clear()
    {
        m_data = 0;
    }

    void Set( CLR_UINT32 idxAssm, CLR_UINT32 idxMethod )
    {
        m_data = idxAssm << 16 | idxMethod;
    }

    //--//

    CLR_IDX Assembly() const { return (CLR_IDX)(m_data >> 16); }
    CLR_IDX Method  () const { return (CLR_IDX)(m_data      ); }
};

struct CLR_RT_ReflectionDef_Index
{
    CLR_UINT16 m_kind;    // CLR_ReflectionType
    CLR_UINT16 m_levels;

    union
    {
        CLR_RT_Assembly_Index  m_assm;
        CLR_RT_TypeDef_Index   m_type;
        CLR_RT_MethodDef_Index m_method;
        CLR_RT_FieldDef_Index  m_field;
        CLR_UINT32             m_raw;
    } m_data;

    //--//

    void Clear();

    CLR_UINT32 GetTypeHash() const;

    void InitializeFromHash( CLR_UINT32 hash );

    CLR_UINT64 GetRawData(                 ) const;
    void       SetRawData( CLR_UINT64 data );

    //--//

    static bool Convert( CLR_RT_HeapBlock& ref, CLR_RT_Assembly_Instance&  inst                     );
    static bool Convert( CLR_RT_HeapBlock& ref, CLR_RT_TypeDef_Instance&   inst, CLR_UINT32* levels );
    static bool Convert( CLR_RT_HeapBlock& ref, CLR_RT_MethodDef_Instance& inst                     );
    static bool Convert( CLR_RT_HeapBlock& ref, CLR_RT_FieldDef_Instance&  inst                     );
    static bool Convert( CLR_RT_HeapBlock& ref, CLR_UINT32&  hash                     );
};


//--//

struct CLR_RT_AssemblyRef_CrossReference
{
    CLR_RT_Assembly* m_target;                        // EVENT HEAP - NO RELOCATION -
};

struct CLR_RT_TypeRef_CrossReference
{
    CLR_RT_TypeDef_Index m_target;
};

struct CLR_RT_FieldRef_CrossReference
{
    CLR_RT_FieldDef_Index m_target;
};

struct CLR_RT_MethodRef_CrossReference
{
    CLR_RT_MethodDef_Index m_target;
};

struct CLR_RT_FieldDef_CrossReference
{
    CLR_IDX m_offset;
};

struct CLR_RT_TypeDef_CrossReference
{
    static const CLR_UINT32 TD_CR_StaticConstructorCalled = 0x0001;
    static const CLR_UINT32 TD_CR_HasFinalizer            = 0x0002;
    static const CLR_UINT32 TD_CR_IsMarshalByRefObject    = 0x0004;

    CLR_UINT16 m_flags;
    CLR_IDX    m_totalFields;
    CLR_UINT32 m_hash;
};

struct CLR_RT_MethodDef_CrossReference
{
    static const CLR_UINT16 MD_CR_Patched   = 0x8000;
    static const CLR_UINT16 MD_CR_OwnerMask = 0x7FFF;

    CLR_UINT16 m_data;

    CLR_IDX    GetOwner () const { return (CLR_IDX)(m_data); }
};

struct CLR_RT_MethodDef_Patch
{
    CLR_IDX m_orig;
    CLR_IDX m_patched;
};

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

struct CLR_RT_MethodDef_DebuggingInfo
{
    static const CLR_UINT8 MD_DI_JustMyCode     = 0x01;
    static const CLR_UINT8 MD_DI_HasBreakpoint  = 0x02;

    CLR_UINT8 m_flags;

    bool IsJMC        () const { return IsFlagSet( MD_DI_JustMyCode )   ; }
    bool HasBreakpoint() const { return IsFlagSet( MD_DI_HasBreakpoint ); }

    void SetJMC       ( bool b )  { SetResetFlags( b, MD_DI_JustMyCode    ); }
    void SetBreakpoint( bool b )  { SetResetFlags( b, MD_DI_HasBreakpoint ); }

private:
    void SetFlags     (         CLR_UINT8 flags )        {         m_flags |=  flags                        ; }
    void ResetFlags   (         CLR_UINT8 flags )        {         m_flags &= ~flags                        ; }
    bool IsFlagSet    (         CLR_UINT8 flags ) const  { return (m_flags &   flags) != 0                  ; }
    void SetResetFlags( bool b, CLR_UINT8 flags )        { if(b) SetFlags( flags ); else ResetFlags( flags ); }
};

#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)


////////////////////////////////////////////////////////////////////////////////////////////////////

#include <TinyCLR_Runtime__HeapBlock.h>

//TODO: Change this to an extern method that is defined in the HAL
#if defined(PLATFORM_WINDOWS) || defined(PLATFORM_WINCE)
    #define SYSTEM_CLOCK_HZ        g_HAL_Configuration_Windows.SystemClock
    #define SLOW_CLOCKS_PER_SECOND g_HAL_Configuration_Windows.SlowClockPerSecond
#endif

////////////////////////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////////

struct CLR_RT_UnicodeHelper
{
    static const CLR_UINT32 SURROGATE_HALFSHIFT   = 10;
    static const CLR_UINT32 SURROGATE_HALFBASE    = 0x00010000;
    static const CLR_UINT32 SURROGATE_HALFMASK    = 0x000003FF;

#ifdef HIGH_SURROGATE_START
#undef HIGH_SURROGATE_START
#undef HIGH_SURROGATE_END
#undef LOW_SURROGATE_START
#undef LOW_SURROGATE_END
#endif
    static const CLR_UINT16 HIGH_SURROGATE_START  = 0xD800;
    static const CLR_UINT16 HIGH_SURROGATE_END    = 0xDBFF;
    static const CLR_UINT16 LOW_SURROGATE_START   = 0xDC00;
    static const CLR_UINT16 LOW_SURROGATE_END     = 0xDFFF;

    const CLR_UINT8*  m_inputUTF8;
    const CLR_UINT16* m_inputUTF16;

    CLR_UINT8*        m_outputUTF8;
    int               m_outputUTF8_size;

    CLR_UINT16*       m_outputUTF16;
    int               m_outputUTF16_size;

    //--//

    void SetInputUTF8 ( LPCSTR            src ) { m_inputUTF8  = (const CLR_UINT8*)src; }
    void SetInputUTF16( const CLR_UINT16* src ) { m_inputUTF16 =                   src; }

    int CountNumberOfCharacters( int max = -1 );
    int CountNumberOfBytes     ( int max = -1 );

    //--//

    bool ConvertFromUTF8( int iMaxChars, bool fJustMove, int iMaxBytes = -1 );
    bool ConvertToUTF8  ( int iMaxChars, bool fJustMove                     );

#if defined(PLATFORM_WINDOWS) || defined(PLATFORM_WINCE)
    static void ConvertToUTF8  ( const std::wstring& src, std:: string& dst );
    static void ConvertFromUTF8( const std:: string& src, std::wstring& dst );
#endif
};

class UnicodeString
{
private:
    CLR_RT_UnicodeHelper  m_unicodeHelper;
    CLR_RT_HeapBlock      m_uHeapBlock;
    CLR_UINT16*           m_wCharArray;
    int                   m_length; /// Length in wide characters (not bytes).

public:
    UnicodeString();
    ~UnicodeString();

    HRESULT Assign( LPCSTR string );
    operator LPCWSTR() { return (LPCWSTR)m_wCharArray; }
    UINT32 Length(){ return m_length; }

private:
    void Release();
};

//--//

struct CLR_RT_ArrayListHelper
{
    static const int c_defaultCapacity = 2;

    static const int FIELD__m_items    = 1;
    static const int FIELD__m_size     = 2;

    //--//

    static HRESULT PrepareArrayList         ( CLR_RT_HeapBlock& pThisRef,                                  int  count, int  capacity );
    static HRESULT ExtractArrayFromArrayList( CLR_RT_HeapBlock& pThisRef, CLR_RT_HeapBlock_Array* & array, int& count, int& capacity );
};

//--//

struct CLR_RT_ByteArrayReader
{
    HRESULT Init( const UINT8* src, UINT32 srcSize );

    HRESULT Read( void* dst, UINT32 size );
    HRESULT Read1Byte( void* dst );

    HRESULT Skip( UINT32 size );

    bool IsValid() { return (source && sourceSize > 0); }

    const UINT8* source;
    UINT32       sourceSize;
};

////////////////////////////////////////////////////////////////////////////////////////////////////

struct CLR_RT_SignatureParser
{
    static const int c_TypeSpec   = 0;
    static const int c_Interfaces = 1;
    static const int c_Field      = 2;
    static const int c_Method     = 3;
    static const int c_Locals     = 4;
    static const int c_Object     = 5;

    struct Element
    {
        bool                 m_fByRef;
        int                  m_levels;
        CLR_DataType         m_dt;
        CLR_RT_TypeDef_Index m_cls;
    };

    CLR_RT_HeapBlock* m_lst;
    CLR_RT_Assembly*  m_assm;
    CLR_PMETADATA     m_sig;

    int               m_type;
    CLR_UINT32        m_flags;
    int               m_count;

    //--//

    void Initialize_TypeSpec       ( CLR_RT_Assembly* assm, const CLR_RECORD_TYPESPEC*  ts );
    void Initialize_Interfaces     ( CLR_RT_Assembly* assm, const CLR_RECORD_TYPEDEF*   td );
    void Initialize_FieldDef       ( CLR_RT_Assembly* assm, const CLR_RECORD_FIELDDEF*  fd );
    void Initialize_MethodSignature( CLR_RT_Assembly* assm, const CLR_RECORD_METHODDEF* md );
    void Initialize_MethodLocals   ( CLR_RT_Assembly* assm, const CLR_RECORD_METHODDEF* md );

    void Initialize_TypeSpec       ( CLR_RT_Assembly* assm, CLR_PMETADATA               ts );
    void Initialize_FieldDef       ( CLR_RT_Assembly* assm, CLR_PMETADATA               fd );
    void Initialize_MethodSignature( CLR_RT_Assembly* assm, CLR_PMETADATA               md );

    void Initialize_Objects        ( CLR_RT_HeapBlock* lst, int count, bool fTypes         );

    int Available() const { return m_count; }

    HRESULT Advance( Element& res );
};

////////////////////////////////////////////////////////////////////////////////////////////////////

#define TINYCLR_FOREACH_ASSEMBLY(ts)                               \
    {                                                              \
        CLR_RT_Assembly** ppASSM = (ts).m_assemblies; \
        size_t iASSM  = (ts).m_assembliesMax;                      \
        for( ;iASSM--; ppASSM++)                                   \
        {                                                          \
            CLR_RT_Assembly* pASSM = *ppASSM;        \
            if(pASSM)

#define TINYCLR_FOREACH_ASSEMBLY_END()                   \
        }                                                \
    }

#define TINYCLR_FOREACH_ASSEMBLY_NULL(ts)                          \
    {                                                              \
        CLR_RT_Assembly** ppASSM = (ts).m_assemblies; \
        size_t            iASSM  = ARRAYSIZE((ts).m_assemblies);   \
        CLR_IDX           idx    = 1;                              \
        for( ;iASSM--; ppASSM++,idx++)                             \
        {                                                          \
            if(*ppASSM == NULL)

#define TINYCLR_FOREACH_ASSEMBLY_NULL_END()                      \
        }                                                        \
    }

#if defined(TINYCLR_APPDOMAINS)
    #define TINYCLR_FOREACH_ASSEMBLY_IN_APPDOMAIN(ad)                                                          \
            TINYCLR_FOREACH_NODE(CLR_RT_AppDomainAssembly, appDomainAssembly, (ad)->m_appDomainAssemblies)     \
            {                                                                                                  \
                CLR_RT_Assembly* pASSM = appDomainAssembly->m_assembly;

    #define TINYCLR_FOREACH_ASSEMBLY_IN_APPDOMAIN_END()                                                        \
            }                                                                                                  \
            TINYCLR_FOREACH_NODE_END()

    #define TINYCLR_FOREACH_ASSEMBLY_IN_CURRENT_APPDOMAIN(ts)   TINYCLR_FOREACH_ASSEMBLY_IN_APPDOMAIN(g_CLR_RT_ExecutionEngine.GetCurrentAppDomain())
    #define TINYCLR_FOREACH_ASSEMBLY_IN_CURRENT_APPDOMAIN_END() TINYCLR_FOREACH_ASSEMBLY_IN_APPDOMAIN_END()
#else
    #define TINYCLR_FOREACH_ASSEMBLY_IN_APPDOMAIN(ts, ad)       TINYCLR_FOREACH_ASSEMBLY(ts)
    #define TINYCLR_FOREACH_ASSEMBLY_IN_APPDOMAIN_END()         TINYCLR_FOREACH_ASSEMBLY_END()
    #define TINYCLR_FOREACH_ASSEMBLY_IN_CURRENT_APPDOMAIN(ts)   TINYCLR_FOREACH_ASSEMBLY(ts)
    #define TINYCLR_FOREACH_ASSEMBLY_IN_CURRENT_APPDOMAIN_END() TINYCLR_FOREACH_ASSEMBLY_END()
#endif
// This type is needed on PC only for Interop code generation. For device code forward declaration only
class CLR_RT_VectorOfManagedElements;

struct CLR_RT_Assembly : public CLR_RT_HeapBlock_Node // EVENT HEAP - NO RELOCATION -
{
    struct Offsets
    {
        size_t iBase;
        size_t iAssemblyRef;
        size_t iTypeRef;
        size_t iFieldRef;
        size_t iMethodRef;
        size_t iTypeDef;
        size_t iFieldDef;
        size_t iMethodDef;

#if !defined(TINYCLR_APPDOMAINS)
        size_t iStaticFields;
#endif

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
        size_t iDebuggingInfoMethods;
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)       
    };

    //--//

    static const CLR_UINT32 c_Resolved                   = 0x00000001;
    static const CLR_UINT32 c_ResolutionCompleted        = 0x00000002;
    static const CLR_UINT32 c_PreparedForExecution       = 0x00000004;
    static const CLR_UINT32 c_Deployed                   = 0x00000008;
    static const CLR_UINT32 c_PreparingForExecution      = 0x00000010;
    static const CLR_UINT32 c_StaticConstructorsExecuted = 0x00000020;

    CLR_UINT32                         m_idx;                         // Relative to the type system (for static fields access).
    CLR_UINT32                         m_flags;

    const CLR_RECORD_ASSEMBLY*         m_header;                      // ANY HEAP - DO RELOCATION -
    LPCSTR                             m_szName;                      // ANY HEAP - DO RELOCATION -

    const CLR_RT_MethodHandler*        m_nativeCode;

#if defined(TINYCLR_JITTER)
    CLR_RT_MethodHandler*              m_jittedCode;
#endif

    int                                m_pTablesSize[ TBL_Max ];

#if !defined(TINYCLR_APPDOMAINS)
    CLR_RT_HeapBlock*                  m_pStaticFields;               // EVENT HEAP - NO RELOCATION - (but the data they point to has to be relocated)
#endif

    int                                m_iStaticFields;

    CLR_RT_HeapBlock_Array*            m_pFile;                       // ANY HEAP - DO RELOCATION -

    CLR_RT_AssemblyRef_CrossReference* m_pCrossReference_AssemblyRef; // EVENT HEAP - NO RELOCATION - (but the data they point to has to be relocated)
    CLR_RT_TypeRef_CrossReference    * m_pCrossReference_TypeRef    ; // EVENT HEAP - NO RELOCATION - (but the data they point to has to be relocated)
    CLR_RT_FieldRef_CrossReference   * m_pCrossReference_FieldRef   ; // EVENT HEAP - NO RELOCATION - (but the data they point to has to be relocated)
    CLR_RT_MethodRef_CrossReference  * m_pCrossReference_MethodRef  ; // EVENT HEAP - NO RELOCATION - (but the data they point to has to be relocated)
    CLR_RT_TypeDef_CrossReference    * m_pCrossReference_TypeDef    ; // EVENT HEAP - NO RELOCATION - (but the data they point to has to be relocated)
    CLR_RT_FieldDef_CrossReference   * m_pCrossReference_FieldDef   ; // EVENT HEAP - NO RELOCATION - (but the data they point to has to be relocated)
    CLR_RT_MethodDef_CrossReference  * m_pCrossReference_MethodDef  ; // EVENT HEAP - NO RELOCATION - (but the data they point to has to be relocated)

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    CLR_RT_MethodDef_DebuggingInfo  * m_pDebuggingInfo_MethodDef   ; //EVENT HEAP - NO RELOCATION - (but the data they point to has to be relocated)
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

#if defined(TINYCLR_TRACE_STACK_HEAVY) && defined(PLATFORM_WINDOWS)
    int                                m_maxOpcodes;
    int*                               m_stackDepth;
#endif


#if defined(PLATFORM_WINDOWS) || defined(PLATFORM_WINCE)
    std::string*                      m_strPath;
#endif

    //--//

    static void InitString();

    bool IsSameAssembly( const CLR_RT_Assembly& assm ) const;
    
#if defined(PLATFORM_WINDOWS)

    static void InitString( std::map<std::string,CLR_OFFSET>& map );
    static HRESULT CreateInstance ( const CLR_RECORD_ASSEMBLY* data, CLR_RT_Assembly*& assm, LPCWSTR szName );

#endif

    //--//

    static HRESULT CreateInstance ( const CLR_RECORD_ASSEMBLY* data, CLR_RT_Assembly*& assm );
    void           DestroyInstance(                                                         );

    void Assembly_Initialize( CLR_RT_Assembly::Offsets& offsets );
    
    bool    Resolve_AssemblyRef           ( bool fOutput );
    HRESULT Resolve_TypeRef               (              );
    HRESULT Resolve_FieldRef              (              );
    HRESULT Resolve_MethodRef             (              );
    void    Resolve_TypeDef               (              );
    void    Resolve_MethodDef             (              );
    void    Resolve_Link                  (              );
    HRESULT Resolve_ComputeHashes         (              );
    HRESULT Resolve_AllocateStaticFields  ( CLR_RT_HeapBlock* pStaticFields );

    static HRESULT VerifyEndian(CLR_RECORD_ASSEMBLY* header);

    HRESULT PrepareForExecution();

    CLR_UINT32 ComputeAssemblyHash(                                  );
    CLR_UINT32 ComputeAssemblyHash( const CLR_RECORD_ASSEMBLYREF* ar );


    bool FindTypeDef  ( LPCSTR     name, LPCSTR  nameSpace                                            , CLR_RT_TypeDef_Index&   idx );
    bool FindTypeDef  ( LPCSTR     name, CLR_IDX scope                                                , CLR_RT_TypeDef_Index&   idx );
    bool FindTypeDef  ( CLR_UINT32 hash                                                               , CLR_RT_TypeDef_Index&   idx );

    bool FindFieldDef ( const CLR_RECORD_TYPEDEF* src, LPCSTR name, CLR_RT_Assembly* base, CLR_SIG sig, CLR_RT_FieldDef_Index&  idx );
    bool FindMethodDef( const CLR_RECORD_TYPEDEF* src, LPCSTR name, CLR_RT_Assembly* base, CLR_SIG sig, CLR_RT_MethodDef_Index& idx );

    bool FindNextStaticConstructor( CLR_RT_MethodDef_Index& idx );

    bool FindMethodBoundaries( CLR_IDX i, CLR_OFFSET& start, CLR_OFFSET& end );

    void Relocate();

    //--//

    CLR_RT_HeapBlock* GetStaticField( const int index );

    //--//

    CLR_PMETADATA GetTable( CLR_TABLESENUM tbl ) { return (CLR_PMETADATA)m_header + m_header->startOfTables[ tbl ]; }

#define TINYCLR_ASSEMBLY_RESOLVE(cls,tbl,idx) (const cls *)((CLR_UINT8*)m_header + m_header->startOfTables[ tbl ] + (sizeof(cls) * idx))
    const CLR_RECORD_ASSEMBLYREF  * GetAssemblyRef ( CLR_IDX    i ) { return TINYCLR_ASSEMBLY_RESOLVE(CLR_RECORD_ASSEMBLYREF  , TBL_AssemblyRef   , i); }
    const CLR_RECORD_TYPEREF      * GetTypeRef     ( CLR_IDX    i ) { return TINYCLR_ASSEMBLY_RESOLVE(CLR_RECORD_TYPEREF      , TBL_TypeRef       , i); }
    const CLR_RECORD_FIELDREF     * GetFieldRef    ( CLR_IDX    i ) { return TINYCLR_ASSEMBLY_RESOLVE(CLR_RECORD_FIELDREF     , TBL_FieldRef      , i); }
    const CLR_RECORD_METHODREF    * GetMethodRef   ( CLR_IDX    i ) { return TINYCLR_ASSEMBLY_RESOLVE(CLR_RECORD_METHODREF    , TBL_MethodRef     , i); }
    const CLR_RECORD_TYPEDEF      * GetTypeDef     ( CLR_IDX    i ) { return TINYCLR_ASSEMBLY_RESOLVE(CLR_RECORD_TYPEDEF      , TBL_TypeDef       , i); }
    const CLR_RECORD_FIELDDEF     * GetFieldDef    ( CLR_IDX    i ) { return TINYCLR_ASSEMBLY_RESOLVE(CLR_RECORD_FIELDDEF     , TBL_FieldDef      , i); }
    const CLR_RECORD_METHODDEF    * GetMethodDef   ( CLR_IDX    i ) { return TINYCLR_ASSEMBLY_RESOLVE(CLR_RECORD_METHODDEF    , TBL_MethodDef     , i); }
    const CLR_RECORD_ATTRIBUTE    * GetAttribute   ( CLR_IDX    i ) { return TINYCLR_ASSEMBLY_RESOLVE(CLR_RECORD_ATTRIBUTE    , TBL_Attributes    , i); }
    const CLR_RECORD_TYPESPEC     * GetTypeSpec    ( CLR_IDX    i ) { return TINYCLR_ASSEMBLY_RESOLVE(CLR_RECORD_TYPESPEC     , TBL_TypeSpec      , i); }
    const CLR_RECORD_RESOURCE_FILE* GetResourceFile( CLR_IDX    i ) { return TINYCLR_ASSEMBLY_RESOLVE(CLR_RECORD_RESOURCE_FILE, TBL_ResourcesFiles, i); }
    const CLR_RECORD_RESOURCE     * GetResource    ( CLR_IDX    i ) { return TINYCLR_ASSEMBLY_RESOLVE(CLR_RECORD_RESOURCE     , TBL_Resources     , i); }
    CLR_PMETADATA                   GetResourceData( CLR_UINT32 i ) { return TINYCLR_ASSEMBLY_RESOLVE(CLR_UINT8               , TBL_ResourcesData , i); }
    LPCSTR                          GetString      ( CLR_STRING i );
    CLR_PMETADATA                   GetSignature   ( CLR_SIG    i ) { return TINYCLR_ASSEMBLY_RESOLVE(CLR_UINT8               , TBL_Signatures    , i); }
    CLR_PMETADATA                   GetByteCode    ( CLR_OFFSET i ) { return TINYCLR_ASSEMBLY_RESOLVE(CLR_UINT8               , TBL_ByteCode      , i); }
#undef TINYCLR_ASSEMBLY_RESOLVE

    //--//

#undef DECL_POSTFIX
#if defined(TINYCLR_TRACE_INSTRUCTIONS)
#define DECL_POSTFIX
#else
#define DECL_POSTFIX {}
#endif
public:
    void DumpOpcode      ( CLR_RT_StackFrame*         stack, CLR_PMETADATA ip                                 ) DECL_POSTFIX;
    void DumpOpcodeDirect( CLR_RT_MethodDef_Instance& call , CLR_PMETADATA ip, CLR_PMETADATA ipStart, int pid ) DECL_POSTFIX;

private:
    void DumpToken         ( CLR_UINT32     tk  ) DECL_POSTFIX;
    void DumpSignature     ( CLR_SIG        sig ) DECL_POSTFIX;
    void DumpSignature     ( CLR_PMETADATA& p   ) DECL_POSTFIX;
    void DumpSignatureToken( CLR_PMETADATA& p   ) DECL_POSTFIX;

    //--//

#if defined(PLATFORM_WINDOWS)
    static FILE* s_output;
    static FILE* s_toclose;

public:
    static void Dump_SetDevice  ( FILE *&outputDeviceFile, LPCWSTR szFileName );
    static void Dump_SetDevice  ( LPCWSTR szFileName      );

    static void Dump_CloseDevice( FILE *&outputDeviceFile);
    static void Dump_CloseDevice(                         );

    static void Dump_Printf     ( FILE *outputDeviceFile, const char *format, ... );
    static void Dump_Printf     ( const char *format, ... );

    static void Dump_Indent( const CLR_RECORD_METHODDEF* md, size_t offset, size_t level );

    void Dump( bool fNoByteCode );

    UINT32 GenerateSignatureForNativeMethods();

    bool AreInternalMethodsPresent( const CLR_RECORD_TYPEDEF* td );
    void GenerateSkeleton( LPCWSTR szFileName, LPCWSTR szProjectName );
    void GenerateSkeletonFromComplientNames( LPCWSTR szFileName, LPCWSTR szProjectName );
    
    void BuildParametersList( CLR_PMETADATA pMetaData, CLR_RT_VectorOfManagedElements &elemPtrArray );
    void GenerateSkeletonStubFieldsDef( const CLR_RECORD_TYPEDEF *pClsType, FILE *pFileStubHead, std::string strIndent, std::string strMngClassName );
    void GenerateSkeletonStubCode( LPCWSTR szFilePath, FILE *pFileDotNetProj );

    void BuildMethodName_Legacy( const CLR_RECORD_METHODDEF* md, std::string& name, CLR_RT_StringMap& mapMethods );
    void GenerateSkeleton_Legacy( LPCWSTR szFileName, LPCWSTR szProjectName );


    void BuildMethodName( const CLR_RECORD_METHODDEF* md, std::string& name    , CLR_RT_StringMap& mapMethods );
    void BuildClassName ( const CLR_RECORD_TYPEREF*   tr, std::string& cls_name, bool              fEscape    );
    void BuildClassName ( const CLR_RECORD_TYPEDEF*   td, std::string& cls_name, bool              fEscape    );
    void BuildTypeName  ( const CLR_RECORD_TYPEDEF*   td, std::string& type_name );

#endif
private:

#if defined(PLATFORM_WINDOWS)
    void Dump_Token         ( CLR_UINT32     tk  );
    void Dump_FieldOwner    ( CLR_UINT32     idx );
    void Dump_MethodOwner   ( CLR_UINT32     idx );
    void Dump_Signature     ( CLR_SIG        sig );
    void Dump_Signature     ( CLR_PMETADATA& p   );
    void Dump_SignatureToken( CLR_PMETADATA& p   );
#endif

    //--//

    PROHIBIT_ALL_CONSTRUCTORS(CLR_RT_Assembly);

    //--//

private:

    CLR_UINT32 ComputeHashForName( const CLR_RT_TypeDef_Index& td, CLR_UINT32 hash );

    static CLR_UINT32 ComputeHashForType( CLR_DataType dt, CLR_UINT32 hash );
};

//--//

#if defined(TINYCLR_APPDOMAINS)

struct CLR_RT_AppDomain : public CLR_RT_ObjectToEvent_Destination // EVENT HEAP - NO RELOCATION -
{
    enum AppDomainState
    {
        AppDomainState_Loaded,
        AppDomainState_Unloading,
        AppDomainState_Unloaded
    };

    AppDomainState            m_state;
    int                       m_id;
    CLR_RT_DblLinkedList      m_appDomainAssemblies;
    CLR_RT_HeapBlock*         m_globalLock;                  // OBJECT HEAP - DO RELOCATION -
    CLR_RT_HeapBlock_String*  m_strName;                     // OBJECT HEAP - DO RELOCATION -
    CLR_RT_HeapBlock*         m_outOfMemoryException;        // OBJECT HEAP - DO RELOCATION -
    CLR_RT_AppDomainAssembly* m_appDomainAssemblyLastAccess; // EVENT HEAP  - NO RELOCATION -
    bool                      m_fCanBeUnloaded;

    static HRESULT CreateInstance ( LPCSTR szName, CLR_RT_AppDomain*& appDomain);

    void DestroyInstance       ();
    void AppDomain_Initialize  ();
    void AppDomain_Uninitialize();
    bool IsLoaded              ();

    void Relocate     ();
    void RecoverFromGC();

    CLR_RT_AppDomainAssembly* FindAppDomainAssembly( CLR_RT_Assembly* assm );

    HRESULT MarshalObject    ( CLR_RT_HeapBlock& src       , CLR_RT_HeapBlock& dst, CLR_RT_AppDomain* appDomainSrc = NULL );
    HRESULT MarshalParameters( CLR_RT_HeapBlock* callerArgs, CLR_RT_HeapBlock* calleeArgs, int count, bool fOnReturn, CLR_RT_AppDomain* appDomainSrc = NULL );

    HRESULT VerifyTypeIsLoaded( const CLR_RT_TypeDef_Index& idx );
    HRESULT GetAssemblies     (       CLR_RT_HeapBlock& ref     );
    HRESULT LoadAssembly      (       CLR_RT_Assembly* assm     );
    HRESULT GetManagedObject  (       CLR_RT_HeapBlock& obj     );
};

struct CLR_RT_AppDomainAssembly : public CLR_RT_HeapBlock_Node //EVENT HEAP - NO RELOCATION -
{
    static const CLR_UINT32 c_StaticConstructorsExecuted = 0x00000001;

    CLR_UINT32        m_flags;
    CLR_RT_AppDomain* m_appDomain;      // EVENT HEAP - NO RELOCATION -
    CLR_RT_Assembly*  m_assembly;       // EVENT HEAP - NO RELOCATION -
    CLR_RT_HeapBlock* m_pStaticFields;  // EVENT HEAP - NO RELOCATION - (but the data they point to has to be relocated)


    static HRESULT CreateInstance( CLR_RT_AppDomain* appDomain, CLR_RT_Assembly* assm, CLR_RT_AppDomainAssembly*& appDomainAssembly );

    void    DestroyInstance();
    HRESULT AppDomainAssembly_Initialize( CLR_RT_AppDomain* appDomain, CLR_RT_Assembly* assm );

    void Relocate();
};

#endif //TINYCLR_APPDOMAINS

//--//

struct CLR_RT_WellKnownTypes
{
    CLR_RT_TypeDef_Index m_Boolean;
    CLR_RT_TypeDef_Index m_Int8;
    CLR_RT_TypeDef_Index m_UInt8;

    CLR_RT_TypeDef_Index m_Char;
    CLR_RT_TypeDef_Index m_Int16;
    CLR_RT_TypeDef_Index m_UInt16;

    CLR_RT_TypeDef_Index m_Int32;
    CLR_RT_TypeDef_Index m_UInt32;
    CLR_RT_TypeDef_Index m_Single;

    CLR_RT_TypeDef_Index m_Int64;
    CLR_RT_TypeDef_Index m_UInt64;
    CLR_RT_TypeDef_Index m_Double;
    CLR_RT_TypeDef_Index m_DateTime;
    CLR_RT_TypeDef_Index m_TimeSpan;
    CLR_RT_TypeDef_Index m_String;

    CLR_RT_TypeDef_Index m_Void;
    CLR_RT_TypeDef_Index m_Object;
    CLR_RT_TypeDef_Index m_ValueType;
    CLR_RT_TypeDef_Index m_Enum;

    CLR_RT_TypeDef_Index m_AppDomainUnloadedException;
    CLR_RT_TypeDef_Index m_ArgumentNullException;
    CLR_RT_TypeDef_Index m_ArgumentException;
    CLR_RT_TypeDef_Index m_ArgumentOutOfRangeException;
    CLR_RT_TypeDef_Index m_Exception;
    CLR_RT_TypeDef_Index m_IndexOutOfRangeException;
    CLR_RT_TypeDef_Index m_ThreadAbortException;
    CLR_RT_TypeDef_Index m_IOException;
    CLR_RT_TypeDef_Index m_InvalidOperationException;
    CLR_RT_TypeDef_Index m_InvalidCastException;
    CLR_RT_TypeDef_Index m_NotSupportedException;
    CLR_RT_TypeDef_Index m_NotImplementedException;
    CLR_RT_TypeDef_Index m_NullReferenceException;
    CLR_RT_TypeDef_Index m_OutOfMemoryException;
    CLR_RT_TypeDef_Index m_ObjectDisposedException;
    CLR_RT_TypeDef_Index m_UnknownTypeException;
    CLR_RT_TypeDef_Index m_ConstraintException;
    CLR_RT_TypeDef_Index m_WatchdogException;

    CLR_RT_TypeDef_Index m_Delegate;
    CLR_RT_TypeDef_Index m_MulticastDelegate;

    CLR_RT_TypeDef_Index m_Array;
    CLR_RT_TypeDef_Index m_ArrayList;
    CLR_RT_TypeDef_Index m_ICloneable;
    CLR_RT_TypeDef_Index m_IList;

    CLR_RT_TypeDef_Index m_Assembly;
    CLR_RT_TypeDef_Index m_TypeStatic;
    CLR_RT_TypeDef_Index m_Type;
    CLR_RT_TypeDef_Index m_ConstructorInfo;
    CLR_RT_TypeDef_Index m_MethodInfo;
    CLR_RT_TypeDef_Index m_FieldInfo;

    CLR_RT_TypeDef_Index m_WeakReference;
    CLR_RT_TypeDef_Index m_ExtendedWeakReference;

    CLR_RT_TypeDef_Index m_SerializationHintsAttribute;

    CLR_RT_TypeDef_Index m_ExtendedTimeZone;

    CLR_RT_TypeDef_Index m_Bitmap;
    CLR_RT_TypeDef_Index m_Font;

    CLR_RT_TypeDef_Index m_TouchEvent;
    CLR_RT_TypeDef_Index m_TouchInput;

    CLR_RT_TypeDef_Index m_Message;

    CLR_RT_TypeDef_Index m_ScreenMetrics;

    CLR_RT_TypeDef_Index m_I2CDevice;
    CLR_RT_TypeDef_Index m_I2CDevice__I2CReadTransaction;
    CLR_RT_TypeDef_Index m_I2CDevice__I2CWriteTransaction;

    CLR_RT_TypeDef_Index m_UsbClientConfiguration;
    CLR_RT_TypeDef_Index m_UsbClientConfiguration__Descriptor;
    CLR_RT_TypeDef_Index m_UsbClientConfiguration__DeviceDescriptor;
    CLR_RT_TypeDef_Index m_UsbClientConfiguration__ClassDescriptor;
    CLR_RT_TypeDef_Index m_UsbClientConfiguration__Endpoint;
    CLR_RT_TypeDef_Index m_UsbClientConfiguration__UsbInterface;
    CLR_RT_TypeDef_Index m_UsbClientConfiguration__ConfigurationDescriptor;
    CLR_RT_TypeDef_Index m_UsbClientConfiguration__StringDescriptor;
    CLR_RT_TypeDef_Index m_UsbClientConfiguration__GenericDescriptor;
    
    CLR_RT_TypeDef_Index m_NetworkInterface;
    CLR_RT_TypeDef_Index m_Wireless80211;

    CLR_RT_TypeDef_Index m_TimeServiceSettings;
    CLR_RT_TypeDef_Index m_TimeServiceStatus;

#if defined(TINYCLR_APPDOMAINS)
    CLR_RT_TypeDef_Index m_AppDomain;
    CLR_RT_TypeDef_Index m_MarshalByRefObject;
#endif

    CLR_RT_TypeDef_Index m_Thread;
    CLR_RT_TypeDef_Index m_ResourceManager;

    CLR_RT_TypeDef_Index m_SocketException;

    CLR_RT_TypeDef_Index m_NativeFileInfo;
    CLR_RT_TypeDef_Index m_VolumeInfo;

    CLR_RT_TypeDef_Index m_XmlNameTable_Entry;
    CLR_RT_TypeDef_Index m_XmlReader_XmlNode;
    CLR_RT_TypeDef_Index m_XmlReader_XmlAttribute;
    CLR_RT_TypeDef_Index m_XmlReader_NamespaceEntry;

    CLR_RT_TypeDef_Index m_CryptoKey;
    CLR_RT_TypeDef_Index m_CryptokiObject;
    CLR_RT_TypeDef_Index m_CryptokiSession;
    CLR_RT_TypeDef_Index m_CryptokiSlot;
    CLR_RT_TypeDef_Index m_CryptokiMechanismType;
    CLR_RT_TypeDef_Index m_CryptoException;
    CLR_RT_TypeDef_Index m_CryptokiCertificate;

    PROHIBIT_COPY_CONSTRUCTORS(CLR_RT_WellKnownTypes);
};

extern CLR_RT_WellKnownTypes g_CLR_RT_WellKnownTypes;

struct CLR_RT_WellKnownMethods
{
    CLR_RT_MethodDef_Index m_ResourceManager_GetObjectFromId;
    CLR_RT_MethodDef_Index m_ResourceManager_GetObjectChunkFromId;

    PROHIBIT_COPY_CONSTRUCTORS(CLR_RT_WellKnownMethods);
};

extern CLR_RT_WellKnownMethods g_CLR_RT_WellKnownMethods;

//--//

typedef void (CLR_RT_HeapBlock::*CLR_RT_HeapBlockRelocate)();

struct CLR_RT_DataTypeLookup
{
    static const CLR_UINT8  c_NA            = 0x00;
    static const CLR_UINT8  c_VariableSize  = 0xFF;

    static const CLR_UINT32 c_Primitive          = 0x00000001;
    static const CLR_UINT32 c_Interface          = 0x00000002;
    static const CLR_UINT32 c_Class              = 0x00000004;
    static const CLR_UINT32 c_ValueType          = 0x00000008;
    static const CLR_UINT32 c_Enum               = 0x00000010;
    static const CLR_UINT32 c_SemanticMask       = 0x0000001F;

    static const CLR_UINT32 c_Array              = 0x00000020;
    static const CLR_UINT32 c_ArrayList          = 0x00000040;
    static const CLR_UINT32 c_SemanticMask2      = 0x0000007F;

    static const CLR_UINT32 c_Reference          = 0x00010000;
    static const CLR_UINT32 c_Numeric            = 0x00020000;
    static const CLR_UINT32 c_Integer            = 0x00040000;
    static const CLR_UINT32 c_Signed             = 0x00080000;
    static const CLR_UINT32 c_Direct             = 0x00100000; // This isn't an indirect reference.
    static const CLR_UINT32 c_OptimizedValueType = 0x00200000; // A value type that is kept in a single HeapBlock.
    static const CLR_UINT32 c_ManagedType        = 0x00400000; // this dt represents a managed type, or a pointer to a managed type
                                                               // More specificly, TypeDescriptor::InitializeFromObject will succeed
                                                               // when starting from an object of with this dt

    CLR_UINT32               m_flags;
    CLR_UINT8                m_sizeInBits;
    CLR_UINT8                m_sizeInBytes;
    CLR_UINT8                m_promoteTo;
    CLR_UINT8                m_convertToElementType;

    CLR_RT_TypeDef_Index*    m_cls;
    CLR_RT_HeapBlockRelocate m_relocate;

#if defined(PLATFORM_WINDOWS) || defined(TINYCLR_TRACE_MEMORY_STATS)
    LPCSTR                   m_name;
#endif   
};

extern const CLR_RT_DataTypeLookup c_CLR_RT_DataTypeLookup[];

//--//

struct CLR_RT_OpcodeLookup
{
    static const CLR_UINT16 COND_BRANCH_NEVER            = 0x0000;
    static const CLR_UINT16 COND_BRANCH_ALWAYS           = 0x0001;
    static const CLR_UINT16 COND_BRANCH_IFTRUE           = 0x0002;
    static const CLR_UINT16 COND_BRANCH_IFFALSE          = 0x0003;
    static const CLR_UINT16 COND_BRANCH_IFEQUAL          = 0x0004;
    static const CLR_UINT16 COND_BRANCH_IFNOTEQUAL       = 0x0005;
    static const CLR_UINT16 COND_BRANCH_IFGREATER        = 0x0006;
    static const CLR_UINT16 COND_BRANCH_IFGREATEROREQUAL = 0x0007;
    static const CLR_UINT16 COND_BRANCH_IFLESS           = 0x0008;
    static const CLR_UINT16 COND_BRANCH_IFLESSOREQUAL    = 0x0009;
    static const CLR_UINT16 COND_BRANCH_IFMATCH          = 0x000A;
    static const CLR_UINT16 COND_BRANCH_THROW            = 0x000B;
    static const CLR_UINT16 COND_BRANCH_MASK             = 0x000F;

    static const CLR_UINT16 COND_OVERFLOW                = 0x0010;
    static const CLR_UINT16 COND_UNSIGNED                = 0x0020;

    static const CLR_UINT16 STACK_RESET                  = 0x0080;

    static const CLR_UINT16 ATTRIB_HAS_TARGET            = 0x0100;
    static const CLR_UINT16 ATTRIB_HAS_DT                = 0x0200;
    static const CLR_UINT16 ATTRIB_HAS_INDEX             = 0x0400;
    static const CLR_UINT16 ATTRIB_HAS_TOKEN             = 0x0800;
    static const CLR_UINT16 ATTRIB_HAS_I4                = 0x1000;
    static const CLR_UINT16 ATTRIB_HAS_R4                = 0x2000;
    static const CLR_UINT16 ATTRIB_HAS_I8                = 0x4000;
    static const CLR_UINT16 ATTRIB_HAS_R8                = 0x8000;

#if defined(TINYCLR_OPCODE_NAMES)
    LPCSTR             m_name;
#endif 

#if defined(TINYCLR_OPCODE_STACKCHANGES)
    CLR_UINT8          m_stackChanges;
#endif

    CLR_OpcodeParam    m_opParam;

#if defined(TINYCLR_JITTER)
    CLR_FlowControl    m_flowCtrl;
#endif

    //--//

#if defined(TINYCLR_OPCODE_PARSER)
    CLR_LOGICAL_OPCODE m_logicalOpcode;
    CLR_DataType       m_dt;
    CLR_INT8           m_index;
    CLR_UINT16         m_flags;
#endif
#if defined(TINYCLR_OPCODE_STACKCHANGES)
    CLR_UINT32 StackPop    () const { return m_stackChanges >>   4;    }
    CLR_UINT32 StackPush   () const { return m_stackChanges &  0xF;    }
    CLR_INT32  StackChanges() const { return StackPush() - StackPop(); }
#endif

#if defined(TINYCLR_OPCODE_NAMES)
    LPCSTR Name() const { return m_name; }
#else
    LPCSTR Name() const { return ""; }
#endif
};

extern const CLR_RT_OpcodeLookup c_CLR_RT_OpcodeLookup[];

//--//

struct CLR_RT_LogicalOpcodeLookup
{
    static const CLR_UINT32 RESTARTPOINT_NEXT      = 0x00000001;
    static const CLR_UINT32 EXCEPTION              = 0x00000002;
    static const CLR_UINT32 EXCEPTION_IF_OVERFLOW  = 0x00000010;
    static const CLR_UINT32 EXCEPTION_IF_ZERO      = 0x00000020;
    static const CLR_UINT32 EXCEPTION_ON_CAST      = 0x00000040;

#if defined(TINYCLR_OPCODE_NAMES)
    LPCSTR     m_name;
#endif

    CLR_UINT32 m_flags;


#if defined(TINYCLR_OPCODE_NAMES)
    LPCSTR Name() const { return m_name; }
#else
    LPCSTR Name() const { return ""; }
#endif
};

extern const CLR_RT_LogicalOpcodeLookup c_CLR_RT_LogicalOpcodeLookup[];

//--//


struct CLR_RT_TypeSystem // EVENT HEAP - NO RELOCATION -
{
    struct CompatibilityLookup
    {
        LPCSTR             name;
        CLR_RECORD_VERSION version;
    };

    //--//

    static const int c_MaxAssemblies = 64;

    //--//

    static const CLR_UINT32 TYPENAME_FLAGS_FULL           = 0x1;
    static const CLR_UINT32 TYPENAME_NESTED_SEPARATOR_DOT = 0x2;

    //--//

    CLR_RT_Assembly*       m_assemblies[ c_MaxAssemblies ];                   // EVENT HEAP - NO RELOCATION - array of CLR_RT_Assembly
    size_t                 m_assembliesMax;
    CLR_RT_Assembly*       m_assemblyMscorlib;
    CLR_RT_Assembly*       m_assemblyNative;

    CLR_RT_MethodDef_Index m_entryPoint;

    //--//

    void TypeSystem_Initialize();
    void TypeSystem_Cleanup();

    void    Link                 ( CLR_RT_Assembly* assm );
    void    PostLinkageProcessing( CLR_RT_Assembly* assm );

    HRESULT ResolveAll               (                   );
    HRESULT PrepareForExecution      (                   );
    HRESULT PrepareForExecutionHelper( LPCSTR szAssembly );

    CLR_RT_Assembly* FindAssembly( LPCSTR     name, const CLR_RECORD_VERSION* ver, bool fExact  );
    
    bool             FindTypeDef ( LPCSTR     name, LPCSTR nameSpace, CLR_RT_Assembly* assm, CLR_RT_TypeDef_Index& res );
    bool             FindTypeDef ( LPCSTR     name,                   CLR_RT_Assembly* assm, CLR_RT_TypeDef_Index& res );
    bool             FindTypeDef ( LPCSTR     name, LPCSTR nameSpace,                        CLR_RT_TypeDef_Index& res );
    bool             FindTypeDef ( CLR_UINT32 hash                  ,                        CLR_RT_TypeDef_Index& res );
    bool             FindTypeDef ( LPCSTR     name                  ,                        CLR_RT_TypeDef_Index& res );
    bool             FindTypeDef ( LPCSTR     name                  , CLR_RT_Assembly* assm, CLR_RT_ReflectionDef_Index& reflex );

    HRESULT LocateResourceFile( CLR_RT_Assembly_Instance assm, LPCSTR name, CLR_INT32& idxResourceFile );
    HRESULT LocateResource    ( CLR_RT_Assembly_Instance assm, CLR_INT32 idxResourceFile, CLR_INT16 id, const CLR_RECORD_RESOURCE*& res, CLR_UINT32& size );

    HRESULT BuildTypeName      ( const CLR_RT_TypeDef_Index&   cls, LPSTR& szBuffer, size_t& size, CLR_UINT32 flags, CLR_UINT32 levels );
    HRESULT BuildTypeName      ( const CLR_RT_TypeDef_Index&   cls, LPSTR& szBuffer, size_t& size                                      );
    HRESULT BuildMethodName    ( const CLR_RT_MethodDef_Index& md , LPSTR& szBuffer, size_t& size                                      );
    HRESULT BuildFieldName     ( const CLR_RT_FieldDef_Index&  fd , LPSTR& szBuffer, size_t& size                                      );
    HRESULT QueueStringToBuffer(                                    LPSTR& szBuffer, size_t& size, LPCSTR szText                       );

    bool FindVirtualMethodDef( const CLR_RT_TypeDef_Index& cls, const CLR_RT_MethodDef_Index& calleeMD                   , CLR_RT_MethodDef_Index& idx );
    bool FindVirtualMethodDef( const CLR_RT_TypeDef_Index& cls, const CLR_RT_MethodDef_Index& calleeMD, LPCSTR calleeName, CLR_RT_MethodDef_Index& idx );

    static bool MatchSignature       ( CLR_RT_SignatureParser& parserLeft      , CLR_RT_SignatureParser& parserRight                             );
    static bool MatchSignatureDirect ( CLR_RT_SignatureParser& parserLeft      , CLR_RT_SignatureParser& parserRight      , bool fIsInstanceOfOK );
    static bool MatchSignatureElement( CLR_RT_SignatureParser::Element& resLeft, CLR_RT_SignatureParser::Element& resRight, bool fIsInstanceOfOK );

    static CLR_DataType MapElementTypeToDataType( CLR_UINT32   et );
    static CLR_UINT32   MapDataTypeToElementType( CLR_DataType dt );

#if defined(PLATFORM_WINDOWS)
    void Dump( LPCWSTR szFileName, bool fNoByteCode );
#endif

    //--//

    PROHIBIT_COPY_CONSTRUCTORS(CLR_RT_TypeSystem);
};

extern CLR_RT_TypeSystem g_CLR_RT_TypeSystem;

//--//

struct CLR_RT_Assembly_Instance : public CLR_RT_Assembly_Index
{
    CLR_RT_Assembly* m_assm;

    //--//

    bool InitializeFromIndex( const CLR_RT_Assembly_Index& idx );
    void Clear              (                                  );
};

struct CLR_RT_TypeSpec_Instance : public CLR_RT_TypeSpec_Index
{
    CLR_RT_Assembly* m_assm;
    CLR_PMETADATA    m_target;

    //--//

    bool InitializeFromIndex( const CLR_RT_TypeSpec_Index& idx );
    void Clear              (                                  );

    bool ResolveToken( CLR_UINT32 tk, CLR_RT_Assembly* assm );
};

//--//

struct CLR_RT_TypeDef_Instance : public CLR_RT_TypeDef_Index
{
    CLR_RT_Assembly*          m_assm;
    const CLR_RECORD_TYPEDEF* m_target;

    //--//

    bool InitializeFromReflection    ( const CLR_RT_ReflectionDef_Index& reflex, CLR_UINT32* levels );
    bool InitializeFromIndex         ( const CLR_RT_TypeDef_Index&       idx                        );
    bool InitializeFromMethod        ( const CLR_RT_MethodDef_Instance&  md                         );
    bool InitializeFromField         ( const CLR_RT_FieldDef_Instance&   fd                         );

    void Clear                       (                                                              );

    bool ResolveToken( CLR_UINT32 tk, CLR_RT_Assembly* assm );

    //--//

    CLR_RT_TypeDef_CrossReference& CrossReference() const { return m_assm->m_pCrossReference_TypeDef[ Type() ]; }

    bool SwitchToParent();
    bool HasFinalizer  () const;

    bool IsATypeHandler();
};

//--//

struct CLR_RT_FieldDef_Instance : public CLR_RT_FieldDef_Index
{
    CLR_RT_Assembly*           m_assm;
    const CLR_RECORD_FIELDDEF* m_target;

    //--//

    bool InitializeFromIndex( const CLR_RT_FieldDef_Index& idx );
    void Clear              (                                  );

    bool ResolveToken( CLR_UINT32 tk, CLR_RT_Assembly* assm );

    //--//

    CLR_RT_FieldDef_CrossReference& CrossReference() const { return m_assm->m_pCrossReference_FieldDef[ Field() ]; }
};

//--//

struct CLR_RT_MethodDef_Instance : public CLR_RT_MethodDef_Index
{
    CLR_RT_Assembly*            m_assm;
    const CLR_RECORD_METHODDEF* m_target;

    //--//

    bool InitializeFromIndex( const CLR_RT_MethodDef_Index& idx );
    void Clear              (                                   );

    bool ResolveToken( CLR_UINT32 tk, CLR_RT_Assembly* assm );

    //--//

    CLR_RT_MethodDef_CrossReference& CrossReference() const { return m_assm->m_pCrossReference_MethodDef[ Method() ]; }
    CLR_UINT32                       Hits()           const { return 0;                                               }

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    CLR_RT_MethodDef_DebuggingInfo& DebuggingInfo() const { return m_assm->m_pDebuggingInfo_MethodDef[ Method() ]; }
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
};


////////////////////////////////////////////////////////////////////////////////////////////////////

struct CLR_RT_AttributeEnumerator
{
    CLR_RT_Assembly*            m_assm;
    const CLR_RECORD_ATTRIBUTE* m_ptr;
    int                         m_num;
    CLR_RECORD_ATTRIBUTE        m_data;

    CLR_RT_MethodDef_Index      m_match;
    CLR_PMETADATA               m_blob;

    void Initialize( const CLR_RT_TypeDef_Instance  & inst );
    void Initialize( const CLR_RT_FieldDef_Instance & inst );
    void Initialize( const CLR_RT_MethodDef_Instance& inst );

    bool Advance();

    bool MatchNext( const CLR_RT_TypeDef_Instance* instTD, const CLR_RT_MethodDef_Instance* instMD );

private:
    void Initialize( CLR_RT_Assembly* assm );
};

struct CLR_RT_AttributeParser
{
    struct Value
    {
        static const int c_ConstructorArgument = 1;
        static const int c_NamedField          = 2;
        static const int c_NamedProperty       = 3;

        int              m_mode;
        CLR_RT_HeapBlock m_value;

        int              m_pos;
        LPCSTR           m_name;
    };

    //--//

    CLR_RT_Assembly*                m_assm;
    CLR_PMETADATA                   m_blob;

    CLR_RT_MethodDef_Instance       m_md;
    CLR_RT_TypeDef_Instance         m_td;
    CLR_RT_SignatureParser          m_parser;
    CLR_RT_SignatureParser::Element m_res;

    int                             m_currentPos;
    int                             m_fixed_Count;
    int                             m_named_Count;
    Value                           m_lastValue;

    //--//

    HRESULT Initialize( const CLR_RT_AttributeEnumerator& en );

    HRESULT Next( Value*& res );

private:
    LPCSTR GetString();
};

////////////////////////////////////////////////////////////////////////////////

struct CLR_RT_TypeDescriptor
{
    CLR_UINT32                 m_flags;
    CLR_RT_TypeDef_Instance    m_handlerCls;

    CLR_RT_ReflectionDef_Index m_reflex;

    CLR_DataType GetDataType() const { return (CLR_DataType)m_handlerCls.m_target->dataType; }

    //--//

    void TypeDescriptor_Initialize();

    HRESULT InitializeFromDataType       ( CLR_DataType                      dt     );
    HRESULT InitializeFromReflection     ( const CLR_RT_ReflectionDef_Index& reflex );
    HRESULT InitializeFromTypeSpec       ( const CLR_RT_TypeSpec_Index&      sig    );
    HRESULT InitializeFromType           ( const CLR_RT_TypeDef_Index&       cls    );
    HRESULT InitializeFromFieldDefinition( const CLR_RT_FieldDef_Instance&   fd     );
    HRESULT InitializeFromSignatureParser( CLR_RT_SignatureParser&           parser );
    HRESULT InitializeFromObject         ( const CLR_RT_HeapBlock&           ref    );

    void ConvertToArray();
    bool ShouldEmitHash();

    bool GetElementType( CLR_RT_TypeDescriptor& sub );

    static HRESULT ExtractTypeIndexFromObject( const CLR_RT_HeapBlock& ref, CLR_RT_TypeDef_Index& res );
    static HRESULT ExtractObjectAndDataType( CLR_RT_HeapBlock*& ref, CLR_DataType& dt );
};

#include <TinyCLR_Runtime__Serialization.h>

////////////////////////////////////////////////////////////////////////////////////////////////////

struct CLR_RT_HeapCluster : public CLR_RT_HeapBlock_Node // EVENT HEAP - NO RELOCATION -
{
    CLR_RT_DblLinkedList   m_freeList;      // list of CLR_RT_HeapBlock_Node
    CLR_RT_HeapBlock_Node* m_payloadStart;
    CLR_RT_HeapBlock_Node* m_payloadEnd;

    //--//

    void HeapCluster_Initialize( CLR_UINT32 size, CLR_UINT32 blockSize ); // Memory is not erased by the caller.

    CLR_RT_HeapBlock* ExtractBlocks( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 length );

    void RecoverFromGC();

    CLR_RT_HeapBlock_Node* InsertInOrder( CLR_RT_HeapBlock_Node* node, CLR_UINT32 size );

    //--//

#undef DECL_POSTFIX
#if TINYCLR_VALIDATE_HEAP >= TINYCLR_VALIDATE_HEAP_1_HeapBlocksAndUnlink
#define DECL_POSTFIX
#else
#define DECL_POSTFIX {}
#endif
    void ValidateBlock( CLR_RT_HeapBlock* ptr ) DECL_POSTFIX;

    //--//

    PROHIBIT_ALL_CONSTRUCTORS(CLR_RT_HeapCluster);
};

//--//

#ifndef TINYCLR_NO_IL_INLINE
struct CLR_RT_InlineFrame
{
    CLR_RT_HeapBlock*         m_locals;     
    CLR_RT_HeapBlock*         m_args;
    CLR_RT_HeapBlock*         m_evalStack;
    CLR_RT_HeapBlock*         m_evalPos;
    CLR_RT_MethodDef_Instance m_call;
    CLR_PMETADATA             m_IP;         
    CLR_PMETADATA             m_IPStart;         
};

struct CLR_RT_InlineBuffer
{
    union
    {
        CLR_RT_InlineBuffer* m_pNext;

        CLR_RT_InlineFrame   m_frame;
    };
};
#endif

struct CLR_RT_StackFrame : public CLR_RT_HeapBlock_Node // EVENT HEAP - NO RELOCATION -
{
    static const int c_OverheadForNewObjOrInteropMethod      =  2; // We need to have more slots in the stack to process a 'newobj' opcode.
    static const int c_MinimumStack                          = 10;


    static const CLR_UINT32 c_MethodKind_Native              = 0x00000000;
    static const CLR_UINT32 c_MethodKind_Interpreted         = 0x00000001;
    static const CLR_UINT32 c_MethodKind_Jitted              = 0x00000002;
    static const CLR_UINT32 c_MethodKind_Mask                = 0x00000003;

    static const CLR_UINT32 c_NativeProfiled                 = 0x00000004;
    static const CLR_UINT32 c_MethodKind_Inlined             = 0x00000008;

    static const CLR_UINT32 c_ExecutingConstructor           = 0x00000010;
    static const CLR_UINT32 c_CompactAndRestartOnOutOfMemory = 0x00000020;
    static const CLR_UINT32 c_CallOnPop                      = 0x00000040;
    static const CLR_UINT32 c_CalledOnPop                    = 0x00000080;

    static const CLR_UINT32 c_NeedToSynchronize              = 0x00000100;
    static const CLR_UINT32 c_PendingSynchronize             = 0x00000200;
    static const CLR_UINT32 c_Synchronized                   = 0x00000400;
    static const CLR_UINT32 c_UNUSED_00000800                = 0x00000800;

    static const CLR_UINT32 c_NeedToSynchronizeGlobally      = 0x00001000;
    static const CLR_UINT32 c_PendingSynchronizeGlobally     = 0x00002000;
    static const CLR_UINT32 c_SynchronizedGlobally           = 0x00004000;
    static const CLR_UINT32 c_UNUSED_00008000                = 0x00008000;

    static const CLR_UINT32 c_ExecutingIL                    = 0x00010000;
    static const CLR_UINT32 c_CallerIsCompatibleForCall      = 0x00020000;
    static const CLR_UINT32 c_CallerIsCompatibleForRet       = 0x00040000;
    static const CLR_UINT32 c_PseudoStackFrameForFilter      = 0x00080000;

    static const CLR_UINT32 c_InlineMethodHasReturnValue     = 0x00100000;
    static const CLR_UINT32 c_UNUSED_00200000                = 0x00200000;
    static const CLR_UINT32 c_UNUSED_00400000                = 0x00400000;
    static const CLR_UINT32 c_UNUSED_00800000                = 0x00800000;

    static const CLR_UINT32 c_UNUSED_01000000                = 0x01000000;
    static const CLR_UINT32 c_UNUSED_02000000                = 0x02000000;

    static const CLR_UINT32 c_AppDomainMethodInvoke          = 0x04000000;
    static const CLR_UINT32 c_AppDomainInjectException       = 0x08000000;
    static const CLR_UINT32 c_AppDomainTransition            = 0x10000000;
    static const CLR_UINT32 c_InvalidIP                      = 0x20000000;
    static const CLR_UINT32 c_UNUSED_40000000                = 0x40000000;
    static const CLR_UINT32 c_HasBreakpoint                  = 0x80000000;

    static const CLR_UINT32 c_ProcessSynchronize             = c_NeedToSynchronize         | c_PendingSynchronize         |
                                                               c_NeedToSynchronizeGlobally | c_PendingSynchronizeGlobally ;

    //--//

    ///////////////////////////////////////////////////////////////////////////////////////////
    //
    // These fields have to be aligned, to speed up the jitter thunk "Internal_Initialize".
    //
    CLR_RT_Thread*            m_owningThread;     // EVENT HEAP - NO RELOCATION -
    CLR_RT_HeapBlock*         m_evalStack;        // EVENT HEAP - NO RELOCATION -
    CLR_RT_HeapBlock*         m_arguments;        // EVENT HEAP - NO RELOCATION -
    CLR_RT_HeapBlock*         m_locals;           // EVENT HEAP - NO RELOCATION -
    CLR_PMETADATA             m_IP;               // ANY   HEAP - DO RELOCATION -
    //
    ///////////////////////////////////////////////////////////////////////////////////////////

    CLR_RT_SubThread*         m_owningSubThread;  // EVENT HEAP - NO RELOCATION -
    CLR_UINT32                m_flags;

    CLR_RT_MethodDef_Instance m_call;

    CLR_RT_MethodHandler      m_nativeMethod;
    CLR_PMETADATA             m_IPstart;          // ANY   HEAP - DO RELOCATION -

    CLR_RT_HeapBlock*         m_evalStackPos;     // EVENT HEAP - NO RELOCATION -
    CLR_RT_HeapBlock*         m_evalStackEnd;     // EVENT HEAP - NO RELOCATION -

    union
    {
        CLR_UINT32            m_customState;
        void*                 m_customPointer;
    };


#ifndef TINYCLR_NO_IL_INLINE
    CLR_RT_InlineBuffer*      m_inlineFrame;
#endif

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    CLR_UINT32                m_depth;
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

#if defined(TINYCLR_PROFILE_NEW_CALLS)
    CLR_PROF_CounterCallChain m_callchain;
#endif

#if defined(TINYCLR_APPDOMAINS)
    CLR_RT_AppDomain*         m_appDomain;
#endif

#if defined(ENABLE_NATIVE_PROFILER)
    bool                      m_fNativeProfiled;
#endif

    CLR_RT_HeapBlock          m_extension[ 1 ];

    ////////////////////////////////////////

    static HRESULT Push( CLR_RT_Thread* th, const CLR_RT_MethodDef_Instance& callInst, CLR_INT32 extraBlocks );

    void Pop();

#ifndef TINYCLR_NO_IL_INLINE
    bool PushInline( CLR_PMETADATA& ip, CLR_RT_Assembly*& assm, CLR_RT_HeapBlock*& evalPos, CLR_RT_MethodDef_Instance& calleeInst, CLR_RT_HeapBlock* pThis);
    void PopInline  ( );

    void RestoreFromInlineStack();
    void RestoreStack(CLR_RT_InlineFrame& frame);
    void SaveStack(CLR_RT_InlineFrame& frame);
#endif 
    

#if defined(TINYCLR_APPDOMAINS)
    static HRESULT PushAppDomainTransition( CLR_RT_Thread* th, const CLR_RT_MethodDef_Instance& callInst, CLR_RT_HeapBlock* pThis, CLR_RT_HeapBlock* pArgs );
           HRESULT  PopAppDomainTransition(                                                                                                                );
#endif

    HRESULT FixCall();
    HRESULT MakeCall ( CLR_RT_MethodDef_Instance md, CLR_RT_HeapBlock* blkThis, CLR_RT_HeapBlock* blkArgs, int nArgs );

    HRESULT HandleSynchronized( bool fAcquire, bool fGlobal );

    void    SetResult        ( CLR_INT32         val, CLR_DataType dataType );
    void    SetResult_I4     ( CLR_INT32         val                        );
    void    SetResult_I8     ( CLR_INT64&        val                        );
    void    SetResult_U4     ( CLR_UINT32        val                        );
    void    SetResult_U8     ( CLR_UINT64&       val                        );

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
    void    SetResult_R4     ( float             val                        );
    void    SetResult_R8     ( double            val                        );
#else
    void    SetResult_R4     ( CLR_INT32         val                        );
    void    SetResult_R8     ( CLR_INT64         val                        );
#endif

    void    SetResult_Boolean( bool              val                        );
    void    SetResult_Object ( CLR_RT_HeapBlock* val                        );
    HRESULT SetResult_String ( LPCSTR            val                        );

    HRESULT SetupTimeout( CLR_RT_HeapBlock& input, CLR_INT64*& output );

    void ConvertResultToBoolean();
    void NegateResult          ();

    HRESULT NotImplementedStub();

    void Relocate();

    ////////////////////////////////////////

    CLR_RT_HeapBlock& ThisRef() const { return m_arguments[ 0 ];        }
    CLR_RT_HeapBlock* This()    const { return ThisRef().Dereference(); }

    CLR_RT_HeapBlock& Arg0() const { return m_arguments[ 0 ]; }
    CLR_RT_HeapBlock& Arg1() const { return m_arguments[ 1 ]; }
    CLR_RT_HeapBlock& Arg2() const { return m_arguments[ 2 ]; }
    CLR_RT_HeapBlock& Arg3() const { return m_arguments[ 3 ]; }
    CLR_RT_HeapBlock& Arg4() const { return m_arguments[ 4 ]; }
    CLR_RT_HeapBlock& Arg5() const { return m_arguments[ 5 ]; }
    CLR_RT_HeapBlock& Arg6() const { return m_arguments[ 6 ]; }
    CLR_RT_HeapBlock& Arg7() const { return m_arguments[ 7 ]; }

    CLR_RT_HeapBlock& ArgN( CLR_INT32 n ) const { return m_arguments[ n ]; }

    CLR_RT_HeapBlock& TopValue         () { return m_evalStackPos[ -1 ]; }
    CLR_RT_HeapBlock& PushValue        () { _ASSERTE(m_evalStackPos < m_evalStackEnd); return *  m_evalStackPos++; }
    CLR_RT_HeapBlock& PopValue         () { _ASSERTE(m_evalStackPos > m_evalStack   ); return *--m_evalStackPos;   }
    void              ResetStack       () {              m_evalStackPos = m_evalStack ; }
    int               TopValuePosition () { return (int)(m_evalStackPos - m_evalStack); }

    CLR_RT_MethodDef_Instance& MethodCall() { return m_call; } 
    
    CLR_RT_HeapBlock& PushValueAndClear()
    {
        CLR_RT_HeapBlock& val = PushValue();

        val.SetObjectReference( NULL );

        return val;
    }

    CLR_RT_HeapBlock& PushValueAndAssign( const CLR_RT_HeapBlock& value )
    {
        CLR_RT_HeapBlock& top = PushValue();

        top.Assign( value );

        return top;
    }


    inline void PushValueI4( CLR_INT32 val ) { SetResult_I4( val ); }

    //--//

    CLR_RT_StackFrame* Caller() { return (CLR_RT_StackFrame*)Prev(); }
    CLR_RT_StackFrame* Callee() { return (CLR_RT_StackFrame*)Next(); }

    PROHIBIT_ALL_CONSTRUCTORS(CLR_RT_StackFrame);
};

//
// This CT_ASSERT macro generates a compiler error in case these fields get out of alignment.
//


CT_ASSERT( offsetof(CLR_RT_StackFrame,m_owningThread) + sizeof(CLR_UINT32) == offsetof(CLR_RT_StackFrame,m_evalStack ) )
CT_ASSERT( offsetof(CLR_RT_StackFrame,m_evalStack   ) + sizeof(CLR_UINT32) == offsetof(CLR_RT_StackFrame,m_arguments ) )
CT_ASSERT( offsetof(CLR_RT_StackFrame,m_arguments   ) + sizeof(CLR_UINT32) == offsetof(CLR_RT_StackFrame,m_locals    ) )
CT_ASSERT( offsetof(CLR_RT_StackFrame,m_locals      ) + sizeof(CLR_UINT32) == offsetof(CLR_RT_StackFrame,m_IP        ) )

////////////////////////////////////////////////////////////////////////////////

struct CLR_RT_ProtectFromGC
{
    static const CLR_UINT32 c_Generic        = 0x00000001;
    static const CLR_UINT32 c_HeapBlock      = 0x00000002;
    static const CLR_UINT32 c_ResetKeepAlive = 0x00000004;

    typedef void (*Callback)( void* state );

    static CLR_RT_ProtectFromGC* s_first;

    CLR_RT_ProtectFromGC*        m_next;
    void**                       m_data;
    Callback                     m_fpn;
    CLR_UINT32                   m_flags;

    CLR_RT_ProtectFromGC ( CLR_RT_HeapBlock& ref     ) { Initialize( ref       ); }
    CLR_RT_ProtectFromGC ( void** data, Callback fpn ) { Initialize( data, fpn ); }
    ~CLR_RT_ProtectFromGC(                           ) { Cleanup   (           ); }

    static void InvokeAll();

private:
    void Initialize( CLR_RT_HeapBlock& ref     );
    void Initialize( void** data, Callback fpn );
    void Cleanup   (                           );

    void Invoke();
};

////////////////////////////////////////

#if defined(TINYCLR_TRACE_EARLYCOLLECTION)

struct CLR_RT_AssertEarlyCollection
{
    static CLR_RT_AssertEarlyCollection* s_first;

    CLR_RT_AssertEarlyCollection* m_next;
    CLR_RT_HeapBlock*             m_ptr;

    CLR_RT_AssertEarlyCollection( CLR_RT_HeapBlock* ptr );
    ~CLR_RT_AssertEarlyCollection();

    void Cancel();

    static void CheckAll( CLR_RT_HeapBlock* ptr );
};

#define TINYCLR_FAULT_ON_EARLY_COLLECTION(ptr) CLR_RT_AssertEarlyCollection  aec##ptr( ptr )
#define TINYCLR_CANCEL_EARLY_COLLECTION(ptr)   aec##ptr.Cancel()
#define TINYCLR_CHECK_EARLY_COLLECTION(ptr)    CLR_RT_AssertEarlyCollection::CheckAll( ptr )

#else

#define TINYCLR_FAULT_ON_EARLY_COLLECTION(ptr)
#define TINYCLR_CANCEL_EARLY_COLLECTION(ptr)
#define TINYCLR_CHECK_EARLY_COLLECTION(ptr)

#endif

////////////////////////////////////////

struct CLR_RT_GarbageCollector
{
    typedef bool (*MarkSingleFtn  )( CLR_RT_HeapBlock** ptr                       );
    typedef bool (*MarkMultipleFtn)( CLR_RT_HeapBlock*  lstExt, CLR_UINT32 numExt );
    typedef bool (*RelocateFtn    )( void**             ref                       );

    struct MarkStackElement
    {
        CLR_RT_HeapBlock* ptr;
        CLR_UINT32        num;
#if defined(TINYCLR_VALIDATE_APPDOMAIN_ISOLATION)
        CLR_RT_AppDomain* appDomain;
#endif
    };

    struct MarkStack : CLR_RT_HeapBlock_Node
    {
        MarkStackElement* m_last;
        MarkStackElement* m_top;

        void Initialize( MarkStackElement* ptr, size_t num );
    };

    struct RelocationRegion
    {
        CLR_UINT8* m_start;
        CLR_UINT8* m_end;
        CLR_UINT8* m_destination;
        CLR_UINT32 m_offset;
    };

    //--//

    static const int        c_minimumSpaceForGC      = 128;
    static const int        c_minimumSpaceForCompact = 128;
    static const CLR_UINT32 c_pressureThreshold      = 10;
    static const CLR_UINT32 c_memoryThreshold        = HEAP_SIZE_THRESHOLD;
    static const CLR_UINT32 c_memoryThreshold2       = HEAP_SIZE_THRESHOLD_UPPER;

    static const CLR_UINT32 c_StartGraphEvent       = 0x00000001;
    static const CLR_UINT32 c_StopGraphEvent        = 0x00000002;
    static const CLR_UINT32 c_DumpGraphHeapEvent    = 0x00000004;
    static const CLR_UINT32 c_DumpPerfCountersEvent = 0x00000008;

    CLR_UINT32            m_numberOfGarbageCollections;
    CLR_UINT32            m_numberOfCompactions;

    CLR_RT_DblLinkedList  m_weakDelegates_Reachable;              // list of CLR_RT_HeapBlock_Delegate_List


    CLR_UINT32            m_totalBytes;
    CLR_UINT32            m_freeBytes;
    CLR_UINT32            m_pressureCounter;

    CLR_RT_DblLinkedList* m_markStackList;
    MarkStack*            m_markStack;

    RelocationRegion*     m_relocBlocks;
    size_t                m_relocTotal;
    size_t                m_relocCount;
    CLR_UINT8*            m_relocMinimum;
    CLR_UINT8*            m_relocMaximum;
#if TINYCLR_VALIDATE_HEAP > TINYCLR_VALIDATE_HEAP_0_None
    RelocateFtn           m_relocWorker;
#endif

    MarkSingleFtn         m_funcSingleBlock;
    MarkMultipleFtn       m_funcMultipleBlocks;

    bool                  m_fOutOfStackSpaceForGC;

#if defined(PLATFORM_WINDOWS) || defined(PLATFORM_WINCE)
    CLR_UINT32            m_events;
#endif

    //--//

    CLR_UINT32 ExecuteGarbageCollection();
    CLR_UINT32 ExecuteCompaction       ();

    void Mark               ();
    void MarkWeak           ();
    void Sweep              ();
    void CheckMemoryPressure();

#if defined(TINYCLR_APPDOMAINS)
    void AppDomain_Mark();
#endif

    void Assembly_Mark();

    void Thread_Mark( CLR_RT_DblLinkedList& threads );
    void Thread_Mark( CLR_RT_Thread*        thread  );

    void Heap_Compact                ();
    CLR_UINT32 Heap_ComputeAliveVsDeadRatio();

    void RecoverEventsFromGC();

    void Heap_Relocate_Prepare ( RelocationRegion* blocks, size_t total            );
    void Heap_Relocate_AddBlock( CLR_UINT8* dst, CLR_UINT8* src, CLR_UINT32 length );
    void Heap_Relocate         (                                                   );

    //--//

    static void Heap_Relocate( CLR_RT_HeapBlock* lst, CLR_UINT32 len );
    static void Heap_Relocate( void**            ref                 );

    //--//

#if TINYCLR_VALIDATE_HEAP >= TINYCLR_VALIDATE_HEAP_3_Compaction

    static bool Relocation_JustCheck( void** ref );

    void ValidatePointers() {  Heap_Relocate_Pass( Relocation_JustCheck ); }

    static void ValidateCluster           ( CLR_RT_HeapCluster*   hc                                           );
    static void ValidateHeap              ( CLR_RT_DblLinkedList& lst                                          );
    static void ValidateBlockNotInFreeList( CLR_RT_DblLinkedList& lst, CLR_RT_HeapBlock_Node* dst              );
    static bool IsBlockInFreeList         ( CLR_RT_DblLinkedList& lst, CLR_RT_HeapBlock_Node* dst, bool fExact );
    static bool IsBlockInHeap             ( CLR_RT_DblLinkedList& lst, CLR_RT_HeapBlock_Node* dst              );

#else

    void ValidatePointers() {}

    static void ValidateCluster           ( CLR_RT_HeapCluster*   hc                              ) {}
    static void ValidateHeap              ( CLR_RT_DblLinkedList& lst                             ) {}
    static void ValidateBlockNotInFreeList( CLR_RT_DblLinkedList& lst, CLR_RT_HeapBlock_Node* dst ) {}

#endif

#if TINYCLR_VALIDATE_HEAP >= TINYCLR_VALIDATE_HEAP_4_CompactionPlus

    struct RelocationRecord
    {
        void**      oldRef;
        CLR_UINT32* oldPtr;

        void**      newRef;
        CLR_UINT32* newPtr;

        CLR_UINT32  data;
    };

    typedef std::list<RelocationRecord*>          Rel_List;
    typedef Rel_List::iterator                    Rel_List_Iter;

    typedef std::map< void**, RelocationRecord* > Rel_Map;
    typedef Rel_Map::iterator                     Rel_Map_Iter;

    static Rel_List s_lstRecords;
    static Rel_Map  s_mapOldToRecord;
    static Rel_Map  s_mapNewToRecord;


    static bool TestPointers_PopulateOld_Worker( void** ref );
    static bool TestPointers_PopulateNew_Worker( void** ref );

    void TestPointers_PopulateOld();
    void TestPointers_Remap      ();
    void TestPointers_PopulateNew();

#else

    void TestPointers_PopulateOld() {}
    void TestPointers_Remap      () {}
    void TestPointers_PopulateNew() {}

#endif

    //--//

#if defined(TINYCLR_GC_VERBOSE)

    void GC_Stats( int& resNumberObjects, int& resSizeObjects, int& resNumberEvents, int& resSizeEvents );

    void DumpThreads   (                        );

#else

    void GC_Stats( int& resNumberObjects, int& resSizeObjects, int& resNumberEvents, int& resSizeEvents )
    {
        resNumberObjects = 0;
        resSizeObjects   = 0;
        resNumberEvents  = 0;
        resSizeEvents    = 0;
    }

    void DumpThreads   (                        ) {}

#endif

    //--//

    bool CheckSingleBlock( CLR_RT_HeapBlock**          ptr ) { return m_funcSingleBlock( ( CLR_RT_HeapBlock* *)ptr ); }
    bool CheckSingleBlock( CLR_RT_HeapBlock_Array**    ptr ) { return m_funcSingleBlock( (CLR_RT_HeapBlock* *)ptr ); }
    bool CheckSingleBlock( CLR_RT_HeapBlock_Delegate** ptr ) { return m_funcSingleBlock( (CLR_RT_HeapBlock* *)ptr ); }

    bool CheckSingleBlock_Force( CLR_RT_HeapBlock* ptr                 ) { return ptr ? m_funcMultipleBlocks( ptr, 1   ) : true; }
    bool CheckMultipleBlocks   ( CLR_RT_HeapBlock* lst, CLR_UINT32 num ) { return lst ? m_funcMultipleBlocks( lst, num ) : true; }

    //--//

    static bool ComputeReachabilityGraphForSingleBlock   ( CLR_RT_HeapBlock** ptr                 );
    static bool ComputeReachabilityGraphForMultipleBlocks( CLR_RT_HeapBlock*  lst, CLR_UINT32 num );

    //--//

    PROHIBIT_COPY_CONSTRUCTORS(CLR_RT_GarbageCollector);

    //--//

private:

    void Heap_Relocate_Pass( RelocateFtn ftn );

    void MarkSlow();
};

extern CLR_RT_GarbageCollector g_CLR_RT_GarbageCollector;

//--//

struct CLR_RT_SubThread : public CLR_RT_HeapBlock_Node // EVENT HEAP - NO RELOCATION -
{
    static const int MODE_IncludeSelf = 0x00000001;
    static const int MODE_CheckLocks  = 0x00000002;

    static const int STATUS_Triggered = 0x00000001;

    CLR_RT_Thread*     m_owningThread;      // EVENT HEAP - NO RELOCATION -
    CLR_RT_StackFrame* m_owningStackFrame;
    CLR_UINT32         m_lockRequestsCount;

    int                m_priority;
    CLR_INT64          m_timeConstraint;
    CLR_UINT32         m_status;

    //--//

    static HRESULT CreateInstance ( CLR_RT_Thread* th, CLR_RT_StackFrame* stack  , int priority, CLR_RT_SubThread*& sth );
    static void    DestroyInstance( CLR_RT_Thread* th, CLR_RT_SubThread*  sthBase, int flags                            );

    bool ChangeLockRequestCount( int diff );

    //--//

    PROHIBIT_ALL_CONSTRUCTORS(CLR_RT_SubThread);
};

//--//

struct CLR_RT_ExceptionHandler
{
    union {
      CLR_RT_TypeDef_Index  m_typeFilter;
      CLR_PMETADATA         m_userFilterStart;
    };
    CLR_PMETADATA           m_tryStart;
    CLR_PMETADATA           m_tryEnd;
    CLR_PMETADATA           m_handlerStart;
    CLR_PMETADATA           m_handlerEnd;

    CLR_UINT16              m_ehType;
    //--//

    bool ConvertFromEH( const CLR_RT_MethodDef_Instance& owner, CLR_PMETADATA ipStart, const CLR_RECORD_EH* ehPtr );

    bool IsCatch()    const { return m_ehType == CLR_RECORD_EH::EH_Catch    ; }
    bool IsCatchAll() const { return m_ehType == CLR_RECORD_EH::EH_CatchAll ; }
    bool IsFilter()   const { return m_ehType == CLR_RECORD_EH::EH_Filter   ; }
    bool IsFinally()  const { return m_ehType == CLR_RECORD_EH::EH_Finally  ; }

};


//
// Directly from the .NET enumerator.
//
struct ThreadPriority
{
    /*=========================================================================
    ** Constants for thread priorities.
    =========================================================================*/
    static const int Lowest      = 0;
    static const int BelowNormal = 1;
    static const int Normal      = 2;
    static const int AboveNormal = 3;
    static const int Highest     = 4;
    // One more priority for internal creation of managed code threads.
    // We do not expose this priority to C# applications.
    static const int System_Highest     = 5;
};

struct CLR_RT_Thread : public CLR_RT_ObjectToEvent_Destination // EVENT HEAP - NO RELOCATION -
{
    typedef void (*ThreadTerminationCallback)( void* arg );

    struct UnwindStack
    {
        CLR_RT_StackFrame*      m_stack;
        CLR_RT_HeapBlock*       m_exception;
        CLR_PMETADATA           m_ip;

        CLR_PMETADATA           m_currentBlockStart;
        CLR_PMETADATA           m_currentBlockEnd;
        CLR_PMETADATA           m_handlerBlockStart;
        CLR_PMETADATA           m_handlerBlockEnd;

        CLR_RT_StackFrame*      m_handlerStack;
        CLR_UINT8               m_flags;

        static const CLR_UINT8 p_Phase_Mask                               = 0x07;
        static const CLR_UINT8 p_1_SearchingForHandler_0                  = 0x01;
        static const CLR_UINT8 p_1_SearchingForHandler_1_SentFirstChance  = 0x02;
        static const CLR_UINT8 p_1_SearchingForHandler_2_SentUsersChance  = 0x03;
        static const CLR_UINT8 p_2_RunningFinallys_0                      = 0x04;
        static const CLR_UINT8 p_2_RunningFinallys_1_SentUnwindBegin      = 0x05;
        static const CLR_UINT8 p_3_RunningHandler                         = 0x06;
        static const CLR_UINT8 p_4_NormalCleanup                          = 0x07;

        static const CLR_UINT8 c_MagicCatchForInline                      = 0x20;
        static const CLR_UINT8 c_MagicCatchForInteceptedException         = 0x40;
        static const CLR_UINT8 c_ContinueExceptionHandler                 = 0x80;

        CLR_UINT8   inline GetPhase()                { return m_flags & p_Phase_Mask; }
        void        inline SetPhase(CLR_UINT8 phase)
        {
            _ASSERTE((phase & ~p_Phase_Mask) == 0);
            m_flags = (m_flags & ~p_Phase_Mask) | phase;
        }

    };

    static const CLR_UINT32 TH_S_Ready      = 0x00000000;
    static const CLR_UINT32 TH_S_Waiting    = 0x00000001;
    static const CLR_UINT32 TH_S_Terminated = 0x00000002;
    static const CLR_UINT32 TH_S_Unstarted  = 0x00000003;

    static const CLR_UINT32 TH_F_Suspended               = 0x00000001;
    static const CLR_UINT32 TH_F_Aborted                 = 0x00000002;
    static const CLR_UINT32 TH_F_System                  = 0x00000004;
    static const CLR_UINT32 TH_F_ContainsDoomedAppDomain = 0x00000008;

    static const CLR_INT32  TH_WAIT_RESULT_INIT          = -1;
    static const CLR_INT32  TH_WAIT_RESULT_HANDLE_0      =  0;
    static const CLR_INT32  TH_WAIT_RESULT_TIMEOUT       = 0x102;  //WaitHandle.WaitTimeout
    static const CLR_INT32  TH_WAIT_RESULT_HANDLE_ALL    = 0x103;

    static const CLR_UINT32 c_TimeQuantum_Milliseconds = 20;
    static const int        c_MaxStackUnwindDepth      = 6;

    int                        m_pid;
    CLR_UINT32                 m_status;
    CLR_UINT32                 m_flags;
    int                        m_executionCounter;
    volatile BOOL              m_timeQuantumExpired;

    CLR_RT_HeapBlock_Delegate* m_dlg;                   // OBJECT HEAP - DO RELOCATION -
    CLR_RT_HeapBlock           m_currentException;      // OBJECT HEAP - DO RELOCATION -
    UnwindStack                m_nestedExceptions[ c_MaxStackUnwindDepth ];
    int                        m_nestedExceptionsPos;

    //--//

    //
    // For example, timers are implemented in terms of Threads. If not NULL, this is a worker thread for a timer.
    //
    ThreadTerminationCallback  m_terminationCallback;
    void*                      m_terminationParameter;  // EVENT HEAP - NO RELOCATION -

    CLR_UINT32                 m_waitForEvents;
    CLR_INT64                  m_waitForEvents_Timeout;
    CLR_INT64                  m_waitForEvents_IdleTimeWorkItem;

    CLR_RT_DblLinkedList       m_locks;                 // EVENT HEAP - NO RELOCATION - list of CLR_RT_HeapBlock_Lock
    CLR_UINT32                 m_lockRequestsCount;

    CLR_RT_HeapBlock_WaitForObject* m_waitForObject;    // EVENT HEAP - NO RELOCATION, but the objects they point to do
    CLR_INT32                       m_waitForObject_Result;

    CLR_RT_DblLinkedList       m_stackFrames;           // EVENT HEAP - NO RELOCATION - list of CLR_RT_StackFrame

    CLR_RT_DblLinkedList       m_subThreads;            // EVENT HEAP - NO RELOCATION - list of CLR_RT_SubThread

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    int                        m_scratchPad;
    bool                       m_fHasJMCStepper;

    CLR_RT_Thread*             m_realThread;          // Normally, this points to the CLR_RT_Thread object that contains it.
                                                      // However, if this thread was spawned on behalf of the debugger to evaluate
                                                      // a property or function call, it points to the object coresponding to the
                                                      // thread that is currently selected in the debugger.
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

#if defined(ENABLE_NATIVE_PROFILER)
    bool                       m_fNativeProfiled;
#endif

    //--//

    static HRESULT CreateInstance( int pid, CLR_RT_HeapBlock_Delegate* pDelegate, int priority, CLR_RT_Thread*& th, CLR_UINT32 flags );
    static HRESULT CreateInstance( int pid, int priority, CLR_RT_Thread*& th, CLR_UINT32 flags );
           HRESULT PushThreadProcDelegate( CLR_RT_HeapBlock_Delegate* pDelegate );

    void DestroyInstance();

    HRESULT Execute  ();

    HRESULT Suspend  ();
    HRESULT Resume   ();
    HRESULT Terminate();
    HRESULT Abort    ();

    void Restart( bool fDeleteEvent );

    void Passivate();

    bool CouldBeActivated();

    void RecoverFromGC();

    void Relocate();

    UnwindStack* PushEH     (                                                                                                                );
    void         PopEH_Inner( CLR_RT_StackFrame* stack, CLR_PMETADATA ip                                                                     );
    bool         FindEhBlock( CLR_RT_StackFrame* stack, CLR_PMETADATA from, CLR_PMETADATA to, CLR_RT_ExceptionHandler& eh, bool onlyFinallys );


    HRESULT      ProcessException                          (                                              );
    HRESULT      ProcessException_EndFilter                (                                              );
    HRESULT      ProcessException_EndFinally               (                                              );
    HRESULT      ProcessException_Phase1                   (                                              );
    HRESULT      ProcessException_Phase2                   (                                              );
    void         ProcessException_FilterPseudoFrameCopyVars(CLR_RT_StackFrame* to, CLR_RT_StackFrame* from);

    static void  ProtectFromGCCallback( void* state );

    static HRESULT Execute_DelegateInvoke( CLR_RT_StackFrame* stack );
    static HRESULT Execute_IL            ( CLR_RT_StackFrame* stack );

    //--//

    CLR_RT_StackFrame* FirstFrame  () const { return (CLR_RT_StackFrame*)m_stackFrames.FirstNode(); }
    CLR_RT_StackFrame* CurrentFrame() const { return (CLR_RT_StackFrame*)m_stackFrames.LastNode (); }

    CLR_RT_SubThread*  CurrentSubThread() const { return (CLR_RT_SubThread*)m_subThreads.LastNode(); }

#if defined(TINYCLR_APPDOMAINS)
    CLR_RT_AppDomain* CurrentAppDomain() const { CLR_RT_StackFrame* stack = CurrentFrame(); return stack->Prev() ? stack->m_appDomain : NULL; }
#endif

    // Just return priority, no way to set it through this function.
    int  GetThreadPriority() const      { return CurrentSubThread()->m_priority; }
    // Here we set it. This function is called if managed code changes thread priority.
    void SetThreadPriority( int threadPri ) { CurrentSubThread()->m_priority = threadPri; }

    int  GetExecutionCounter() const { return CurrentSubThread()->m_priority + m_executionCounter; }

    // QuantumDebit is update for execution counter for each quantum:
    // System_Highest       - 1
    // Highest              - 2
    // Above Normal         - 4
    // Normal               - 8
    // Below Normal         - 16
    // Lowest               - 32
    int  GetQuantumDebit()  const { return 1 << ( ThreadPriority::System_Highest - GetThreadPriority() ); }

    // If thread was sleeping and get too far behind on updating of m_executionCounter
    // Then we make m_executionCounter 4 quantums above m_GlobalExecutionCounter;
    void BringExecCounterToDate( int iGlobalExecutionCounter, int iDebitForEachRun );

    void PopEH( CLR_RT_StackFrame* stack, CLR_PMETADATA ip ) { if(m_nestedExceptionsPos) PopEH_Inner( stack, ip ); }

#if defined(TINYCLR_TRACE_CALLS)
    void DumpStack();
#else
    void DumpStack() {}
#endif

    bool IsFinalizerThread();
    bool ReleaseWhenDeadEx();
    void OnThreadTerminated();
    bool CanThreadBeReused();

    //--//

    PROHIBIT_ALL_CONSTRUCTORS(CLR_RT_Thread);

    //--//

private:

    HRESULT Execute_Inner();
};

////////////////////////////////////////////////////////////////////////////////

extern size_t LinkArraySize   ();
extern size_t LinkMRUArraySize();
extern size_t PayloadArraySize();
extern size_t InterruptRecords();
#ifndef TINYCLR_NO_IL_INLINE
extern size_t InlineBufferCount();
#endif


extern CLR_UINT32 g_scratchVirtualMethodTableLink     [];
extern CLR_UINT32 g_scratchVirtualMethodTableLinkMRU  [];
extern CLR_UINT32 g_scratchVirtualMethodPayload       [];
extern CLR_UINT32 g_scratchInterruptDispatchingStorage[];
#ifndef TINYCLR_NO_IL_INLINE
extern CLR_UINT32 g_scratchInlineBuffer               [];
#endif

////////////////////////////////////////////////////////////////////////////////

struct CLR_RT_EventCache
{
    struct BoundedList
    {
        CLR_RT_DblLinkedList m_blocks;
    };

#if defined(TINYCLR_USE_AVLTREE_FOR_METHODLOOKUP)

    struct Payload
    {
        //
        // The first two fields compose the key, the last field is the value.
        //
        CLR_RT_MethodDef_Index m_mdVirtual; // The definition of the virtual method.
        CLR_RT_TypeDef_Index   m_cls;       // The class of the instance we need to resolve.
        CLR_RT_MethodDef_Index m_md;        // The actual implementation of the virtual method.

        //--//

        int Compare( Payload& right )
        {
            if(m_mdVirtual.m_data == right.m_mdVirtual.m_data)
            {
                if(m_cls.m_data == right.m_cls.m_data) return 0;

                return m_cls.m_data < right.m_cls.m_data ? -1 : 1;
            }

            return m_mdVirtual.m_data < right.m_mdVirtual.m_data ? -1 : 1;
        }
    };

    struct LookupEntry : public CLR_RT_AVLTree::Entry
    {
        Payload m_payload;

        //--//

        static int Callback_Compare( void* state, CLR_RT_AVLTree::Entry* left, CLR_RT_AVLTree::Entry* right );
    };

    struct VirtualMethodTable
    {
        CLR_RT_AVLTree       m_tree;
        LookupEntry*         m_entries;

        CLR_RT_DblLinkedList m_list_freeItems;     // list of CLR_RT_EventCache::LookupEntry
        CLR_RT_DblLinkedList m_list_inUse;         // list of CLR_RT_EventCache::LookupEntry

        //--//

        void Initialize();

        bool FindVirtualMethod( const CLR_RT_TypeDef_Index& cls, const CLR_RT_MethodDef_Index& mdVirtual, CLR_RT_MethodDef_Index& md );

        //--//

        static CLR_RT_AVLTree::Entry* Callback_NewNode ( void* state, CLR_RT_AVLTree::Entry* payload                         );
        static void                   Callback_FreeNode( void* state, CLR_RT_AVLTree::Entry* node                            );
        static void                   Callback_Reassign( void* state, CLR_RT_AVLTree::Entry* from, CLR_RT_AVLTree::Entry* to );

        //--//

#if defined(PLATFORM_WINDOWS)
        void DumpTree        (                               );
        bool ConsistencyCheck(                               );
        bool ConsistencyCheck( LookupEntry* node, int& depth );
#else
        void DumpTree        () {              }
        bool ConsistencyCheck() { return true; }
#endif
    };

#else

    struct Link
    {
        CLR_UINT16 m_next;
        CLR_UINT16 m_prev;
    };

    struct Payload
    {
        struct Key
        {
            CLR_RT_MethodDef_Index m_mdVirtual; // The definition of the virtual method.
            CLR_RT_TypeDef_Index   m_cls;       // The class of the instance we need to resolve.
        };

        Key                    m_key;
        CLR_RT_MethodDef_Index m_md;        // The actual implementation of the virtual method.
    };

    struct VirtualMethodTable
    {
        Link*    m_entries;
        Link*    m_entriesMRU;
        Payload* m_payloads;

        //--//

        void Initialize();

        bool FindVirtualMethod( const CLR_RT_TypeDef_Index& cls, const CLR_RT_MethodDef_Index& mdVirtual, CLR_RT_MethodDef_Index& md );

    private:

        CLR_UINT32 GetNewEntry() { return m_entriesMRU[ LinkMRUArraySize() - 1 ].m_prev; }

        static void MoveEntryToTop( Link* entries, CLR_UINT32 slot, CLR_UINT32 idx );
    };
#endif

    //--//

    static const CLR_UINT16 c_maxFastLists = 40;

    // the scratch array is used to avoid bringing in arm ABI methods (for semihosting)
    // struct arrays require initialization with the v3.0 compiler and this is done with ABI methods,
    // unless of course you provide a work around lik this ;-)
    UINT32                  m_scratch[ (sizeof(BoundedList) * c_maxFastLists + 3) / sizeof(UINT32) ];
    BoundedList*            m_events;

    VirtualMethodTable      m_lookup_VirtualMethod;
#ifndef TINYCLR_NO_IL_INLINE
    CLR_RT_InlineBuffer*    m_inlineBufferStart;
#endif

    //--//

    void       EventCache_Initialize();
    CLR_UINT32 EventCache_Cleanup   ();

    void              Append_Node       ( CLR_RT_HeapBlock* node                                   );
    CLR_RT_HeapBlock* Extract_Node_Slow ( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 blocks );
    CLR_RT_HeapBlock* Extract_Node_Fast ( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 blocks );
    CLR_RT_HeapBlock* Extract_Node_Bytes( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 bytes  );
    CLR_RT_HeapBlock* Extract_Node      ( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 blocks );

    bool FindVirtualMethod( const CLR_RT_TypeDef_Index& cls, const CLR_RT_MethodDef_Index& mdVirtual, CLR_RT_MethodDef_Index& md );

#ifndef TINYCLR_NO_IL_INLINE
    bool GetInlineFrameBuffer(CLR_RT_InlineBuffer** ppBuffer);
    bool FreeInlineBuffer(CLR_RT_InlineBuffer* pBuffer);
#endif

    //--//

#define EVENTCACHE_EXTRACT_NODE_AS_BYTES(ev,cls,type,flags,size)  (cls*)((ev).Extract_Node_Bytes( type, flags, size ))
#define EVENTCACHE_EXTRACT_NODE_AS_BLOCKS(ev,cls,type,flags,size) (cls*)((ev).Extract_Node( type, flags, size ))
#define EVENTCACHE_EXTRACT_NODE(ev,cls,type)                      EVENTCACHE_EXTRACT_NODE_AS_BLOCKS(ev,cls,type,                                    0,CONVERTFROMSIZETOHEAPBLOCKS(sizeof(cls)))
#define EVENTCACHE_EXTRACT_NODE_NOALLOC(ev,cls,type)              EVENTCACHE_EXTRACT_NODE_AS_BLOCKS(ev,cls,type,CLR_RT_HeapBlock::HB_NoGcOnFailure   ,CONVERTFROMSIZETOHEAPBLOCKS(sizeof(cls)))
#define EVENTCACHE_EXTRACT_NODE_INITTOZERO(ev,cls,type)           EVENTCACHE_EXTRACT_NODE_AS_BLOCKS(ev,cls,type,CLR_RT_HeapBlock::HB_InitializeToZero,CONVERTFROMSIZETOHEAPBLOCKS(sizeof(cls)))

    //--//

    PROHIBIT_COPY_CONSTRUCTORS(CLR_RT_EventCache);
};

extern CLR_RT_EventCache g_CLR_RT_EventCache;

//////////////////////////////////////////////////////////////////////////////////////
// keep under control the size of the Link and Payload, since we will use externally
// defined arrays to handle those data structures in the Virtual Method cache

#if defined(TINYCLR_USE_AVLTREE_FOR_METHODLOOKUP)
CT_ASSERT( sizeof(CLR_RT_EventCache::LookupEntry) == 12 )
#else
CT_ASSERT( sizeof(CLR_RT_EventCache::Link)        ==  4 )
CT_ASSERT( sizeof(CLR_RT_EventCache::Payload)     == 12 )
#endif

//////////////////////////////////////////////////////////////////////////////////////

//--//

#include <TinyCLR_Debugging.h>
#include <TinyCLR_Profiling.h>
#include <TinyCLR_Messaging.h>

//--//

#if defined(TINYCLR_TRACE_STACK)

//
// If this is set, no memory allocation should be allowed, it could lead to a GC while in an inconsistent state!!
//
extern bool g_CLR_RT_fBadStack;

#endif

//--//

struct CLR_RT_ExecutionEngine
{
    static const CLR_UINT32             c_Event_SerialPort  = 0x00000002;
    static const CLR_UINT32             c_Event_Battery     = 0x00000008;
    static const CLR_UINT32             c_Event_AirPressure = 0x00000010;
    static const CLR_UINT32             c_Event_HeartRate   = 0x00000020;
    static const CLR_UINT32             c_Event_I2C         = 0x00000040;
    static const CLR_UINT32             c_Event_IO          = 0x00000080;
    static const CLR_UINT32             c_Event_EndPoint    = 0x01000000;
    static const CLR_UINT32             c_Event_AppDomain   = 0x02000000;
    static const CLR_UINT32             c_Event_Socket      = 0x20000000;
    static const CLR_UINT32             c_Event_IdleCPU     = 0x40000000;
    static const CLR_UINT32             c_Event_LowMemory   = 0x80000000; // Wait for a low-memory condition.


    ////////////////////////////////////////////////////////////////////////////////////////////////

    static const CLR_UINT32             c_Compile_CPP      = 0x00000001;
    static const CLR_UINT32             c_Compile_ARM      = 0x00000002;

    ////////////////////////////////////////////////////////////////////////////////////////////////

    static const int                    c_HeapState_Normal         = 0x00000000;
    static const int                    c_HeapState_UnderGC        = 0x00000001;

    ////////////////////////////////////////////////////////////////////////////////////////////////

    static const int                    c_fDebugger_Unused00000001           = 0x00000001;
    static const int                    c_fDebugger_Unused00000002           = 0x00000002;
    static const int                    c_fDebugger_Unused00000004           = 0x00000004;
    //
    static const int                    c_fDebugger_LcdSendFrame             = 0x00000100;
    static const int                    c_fDebugger_LcdSendFrameNotification = 0x00000200;
    //
    static const int                    c_fDebugger_State_Initialize         = 0x00000000;
    static const int                    c_fDebugger_State_ProgramRunning     = 0x00000400;
    static const int                    c_fDebugger_State_ProgramExited      = 0x00000800;
    static const int                    c_fDebugger_State_Mask               = 0x00000c00;
    //
    static const int                    c_fDebugger_BreakpointsDisabled      = 0x00001000;
    //
    static const int                    c_fDebugger_Quiet                    = 0x00010000; // Do not spew debug text to the debugger
    static const int                    c_fDebugger_ExitPending              = 0x00020000;
    //
    static const int                    c_fDebugger_PauseTimers              = 0x04000000; // Threads associated with timers are created in "suspended" mode.
    static const int                    c_fDebugger_NoCompaction             = 0x08000000; // Don't perform compaction during execution.
    //
    static const int                    c_fDebugger_SourceLevelDebugging     = 0x10000000;
    static const int                    c_fDebugger_RebootPending            = 0x20000000;
    static const int                    c_fDebugger_Enabled                  = 0x40000000;
    static const int                    c_fDebugger_Stopped                  = 0x80000000;

    volatile int                        m_iDebugger_Conditions;

    ////////////////////////////////////////////////////////////////////////////////////////////////

    static const int                    c_fExecution_GC_Pending              = 0x00000001; //Not currently used
    static const int                    c_fExecution_Compaction_Pending      = 0x00000002;
#if defined(TINYCLR_APPDOMAINS)
    static const int                    c_fExecution_UnloadingAppDomain      = 0x00000004;
#endif

    int                                 m_iExecution_Conditions;

    static const int                    c_fReboot_Normal                     = 0x00000000;
    static const int                    c_fReboot_ClrOnly                    = 0x00000001;
    static const int                    c_fReboot_EnterBootLoader            = 0x00000002;
    static const int                    c_fReboot_ClrOnlyStopDebugger        = 0x00000004 | c_fReboot_ClrOnly;

    int                                 m_iReboot_Options;

    ////////////////////////////////////////////////////////////////////////////////////////////////

    static const int                    c_fProfiling_Enabled                 = 0x00000001;
    static const int                    c_fProfiling_Allocations             = 0x00000002;
    static const int                    c_fProfiling_Calls                   = 0x00000004;
    static const int                    c_fProfiling_TinyCLRTypes            = 0x00000008;  //Don't dump out certain types, like ASSEMBLY, or THREAD, or BINARY_BLOB, etc.
    int                                 m_iProfiling_Conditions;

    enum
    {   EXECUTION_COUNTER_MAXIMUM    = 0x70000000, // Threshold value when we increase all execution counters to avoid overflow
        EXECUTION_COUNTER_ADJUSTMENT = 0x60000000  // The update ( increase ) value for all executioin counters after threshold is reached.
    };
    ////////////////////////////////////////////////////////////////////////////////////////////////

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    static const int                    c_MaxBreakpointsActive               = 5;

    size_t                                               m_breakpointsNum;
    CLR_DBG_Commands::Debugging_Execution_BreakpointDef* m_breakpoints;
    CLR_DBG_Commands::Debugging_Execution_BreakpointDef  m_breakpointsActive[ c_MaxBreakpointsActive ];
    size_t                                               m_breakpointsActiveNum;
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

#if !defined(BUILD_RTM) || defined(PLATFORM_WINDOWS)
    bool m_fPerformGarbageCollection;   //Should the EE do a GC every context switch
    bool m_fPerformHeapCompaction;      //Should the EE do a Compaction following every GC
#endif

#if defined(TINYCLR_APPDOMAINS)
    static const int                    c_AppDomainId_Invalid                = 0;

    CLR_RT_DblLinkedList   m_appDomains;
    CLR_RT_AppDomain*      m_appDomainCurrent;
    int                    m_appDomainIdNext;

    CLR_RT_AppDomain* SetCurrentAppDomain( CLR_RT_AppDomain* appDomain );
    CLR_RT_AppDomain* GetCurrentAppDomain(                             );

    HRESULT UnloadAppDomain              ( CLR_RT_AppDomain* appDomain, CLR_RT_Thread*        th      );
    void PrepareThreadsForAppDomainUnload( CLR_RT_AppDomain* appDomain, CLR_RT_DblLinkedList& threads );

    void TryToUnloadAppDomains_Helper_Threads   ( CLR_RT_DblLinkedList& threads                 );
    void TryToUnloadAppDomains_Helper_Finalizers( CLR_RT_DblLinkedList& finalizers, bool fAlive );
    bool TryToUnloadAppDomains                  (                                               );
#endif //TINYCLR_APPDOMAINS

#define CLR_EE_DBG_IS( Cond )             ((g_CLR_RT_ExecutionEngine.m_iDebugger_Conditions &   CLR_RT_ExecutionEngine::c_fDebugger_##Cond) != 0)
#define CLR_EE_DBG_IS_NOT( Cond )         ((g_CLR_RT_ExecutionEngine.m_iDebugger_Conditions &   CLR_RT_ExecutionEngine::c_fDebugger_##Cond) == 0)
#define CLR_EE_DBG_SET( Cond )              g_CLR_RT_ExecutionEngine.m_iDebugger_Conditions |=  CLR_RT_ExecutionEngine::c_fDebugger_##Cond
#define CLR_EE_DBG_CLR( Cond )              g_CLR_RT_ExecutionEngine.m_iDebugger_Conditions &= ~CLR_RT_ExecutionEngine::c_fDebugger_##Cond
#define CLR_EE_DBG_SET_MASK( Cond, Mask ) CLR_EE_DBG_CLR( Mask ); CLR_EE_DBG_SET( Cond );
#define CLR_EE_DBG_RESTORE( Cond, f )     ((f) ? CLR_EE_DBG_SET( Cond ) : CLR_EE_DBG_CLR( Cond ) )
#define CLR_EE_DBG_IS_MASK( Cond, Mask )  ((g_CLR_RT_ExecutionEngine.m_iDebugger_Conditions  & CLR_RT_ExecutionEngine::c_fDebugger_##Mask) == CLR_RT_ExecutionEngine::c_fDebugger_##Cond)

#define CLR_EE_PRF_IS( Cond )             ((g_CLR_RT_ExecutionEngine.m_iProfiling_Conditions & CLR_RT_ExecutionEngine::c_fProfiling_##Cond) != 0)
#define CLR_EE_PRF_IS_NOT( Cond )         ((g_CLR_RT_ExecutionEngine.m_iProfiling_Conditions & CLR_RT_ExecutionEngine::c_fProfiling_##Cond) == 0)

#define CLR_EE_IS( Cond )             ((g_CLR_RT_ExecutionEngine.m_iExecution_Conditions &   CLR_RT_ExecutionEngine::c_fExecution_##Cond) != 0)
#define CLR_EE_IS_NOT( Cond )         ((g_CLR_RT_ExecutionEngine.m_iExecution_Conditions &   CLR_RT_ExecutionEngine::c_fExecution_##Cond) == 0)
#define CLR_EE_SET( Cond )              g_CLR_RT_ExecutionEngine.m_iExecution_Conditions |=  CLR_RT_ExecutionEngine::c_fExecution_##Cond
#define CLR_EE_CLR( Cond )              g_CLR_RT_ExecutionEngine.m_iExecution_Conditions &= ~CLR_RT_ExecutionEngine::c_fExecution_##Cond

#define CLR_EE_REBOOT_IS( Cond )      ((g_CLR_RT_ExecutionEngine.m_iReboot_Options &   CLR_RT_ExecutionEngine::c_fReboot_##Cond) == CLR_RT_ExecutionEngine::c_fReboot_##Cond)
#define CLR_EE_REBOOT_SET( Cond )       g_CLR_RT_ExecutionEngine.m_iReboot_Options |=  CLR_RT_ExecutionEngine::c_fReboot_##Cond
#define CLR_EE_REBOOT_CLR( Cond )       g_CLR_RT_ExecutionEngine.m_iReboot_Options &= ~CLR_RT_ExecutionEngine::c_fReboot_##Cond

#define CLR_EE_DBG_EVENT_SEND( cmd, size, payload, flags ) ((g_CLR_DBG_Debuggers[ DEBUGGER_PORT_INDEX ].m_messaging != NULL) ? g_CLR_DBG_Debuggers[ DEBUGGER_PORT_INDEX ].m_messaging->SendEvent( cmd, size, (UINT8*)payload, flags ) : false)

#if NUM_MESSAGING == 1
    #define CLR_EE_MSG_EVENT_RPC( cmd, size, payload, flags ) g_CLR_Messaging[ 0 ].SendEvent( cmd, size, (UINT8*)payload, flags )
#else
    #define CLR_EE_MSG_EVENT_RPC( cmd, size, payload, flags ) CLR_Messaging::BroadcastEvent( cmd, size, (UINT8*)payload, flags )
#endif

#if NUM_DEBUGGERS == 1
    #define CLR_EE_DBG_EVENT_BROADCAST( cmd, size, payload, flags ) CLR_EE_DBG_EVENT_SEND( cmd, size, payload, flags )
#else
    #define CLR_EE_DBG_EVENT_BROADCAST( cmd, size, payload, flags ) CLR_DBG_Debugger::BroadcastEvent( cmd, size, (UINT8*)payload, flags )
#endif

    ////////////////////////////////////////////////////////////////////////////////////////////////

    //
    // Used to subtract system time (GC, compaction, other) from ExecutionConstraint checks.
    //
    struct ExecutionConstraintCompensation
    {
        CLR_INT32  m_recursion;
        CLR_INT64  m_start;
        CLR_INT64  m_cumulative;

        void Suspend()
        {
            if(m_recursion++ == 0)
            {
                m_start = HAL_Time_CurrentTicks();
            }
        }

        void Resume()
        {
            if(m_recursion)
            {
                if(--m_recursion == 0)
                {
                    m_cumulative += (HAL_Time_CurrentTicks() - m_start);
                }
            }
        }

        CLR_INT64 Adjust( CLR_INT64 time ) const
        {
            return time + ::HAL_Time_TicksToTime( m_cumulative );
        }
    };

    static ExecutionConstraintCompensation s_compensation;

    //--//
    
    CLR_INT64                           m_maximumTimeToActive;
    
    //--//
    
    CLR_INT64                           m_currentMachineTime;
    CLR_INT64                           m_currentLocalTime;
    CLR_INT64                           m_startTime;  
    CLR_INT32                           m_lastTimeZoneOffset;
    CLR_INT64                           m_currentNextActivityTime;
    bool                                m_timerCache;
    CLR_INT64                           m_timerCacheNextTimeout;

    CLR_RT_DblLinkedList                m_heap;                 // list of CLR_RT_HeapCluster
    CLR_RT_HeapCluster*                 m_lastHcUsed;
    int                                 m_heapState;

    CLR_RT_DblLinkedList                m_weakReferences;       // OBJECT HEAP - DO RELOCATION - list of CLR_RT_HeapBlock_WeakReference

    CLR_RT_DblLinkedList                m_timers;               // EVENT HEAP - NO RELOCATION - list of CLR_RT_HeapBlock_Timer
    CLR_UINT32                          m_raisedEvents;

    CLR_RT_DblLinkedList                m_threadsReady;         // EVENT HEAP - NO RELOCATION - list of CLR_RT_Thread
    CLR_RT_DblLinkedList                m_threadsWaiting;       // EVENT HEAP - NO RELOCATION - list of CLR_RT_Thread
    CLR_RT_DblLinkedList                m_threadsZombie;        // EVENT HEAP - NO RELOCATION - list of CLR_RT_Thread
    int                                 m_lastPid;
    CLR_RT_Thread*                      m_currentThread;

    CLR_RT_DblLinkedList                m_finalizersAlive;      // EVENT HEAP - NO RELOCATION - list of CLR_RT_HeapBlock_Finalizer
    CLR_RT_DblLinkedList                m_finalizersPending;    // EVENT HEAP - NO RELOCATION - list of CLR_RT_HeapBlock_Finalizer
    CLR_RT_Thread*                      m_finalizerThread;      // EVENT HEAP - NO RELOCATION -
    CLR_RT_Thread*                      m_cctorThread;          // EVENT HEAP - NO RELOCATION -

#if !defined(TINYCLR_APPDOMAINS)
    CLR_RT_HeapBlock*                   m_globalLock;           // OBJECT HEAP - DO RELOCATION -
    CLR_RT_HeapBlock*                   m_outOfMemoryException; // OBJECT HEAP - DO RELOCATION -
#endif

    CLR_RT_HeapBlock*                   m_currentUICulture;     // OBJECT HEAP - DO RELOCATION -

    //--//

    CLR_RT_Thread*                      m_interruptThread;      // EVENT HEAP - NO RELOCATION
    CLR_RT_Thread*                      m_timerThread;          // EVENT HEAP - NO RELOCATION

    //--//

#if defined(TINYCLR_JITTER)
    const FLASH_SECTOR*                 m_jitter_firstSector;
    int                                 m_jitter_numSectors;

    FLASH_WORD*                         m_jitter_current;
    FLASH_WORD*                         m_jitter_end;
#endif

    //--//

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    CLR_RT_HeapBlock_Array*             m_scratchPadArray;      // OBJECT HEAP - DO RELOCATION -
#endif

    bool                                m_fShuttingDown;

    //--//

    static HRESULT CreateInstance();

    HRESULT ExecutionEngine_Initialize();

    static HRESULT DeleteInstance();

    void ExecutionEngine_Cleanup();

    HRESULT StartHardware();

    static void Reboot( bool fHard );

    void JoinAllThreadsAndExecuteFinalizer();
    
    void LoadDownloadedAssemblies();

    static void ExecutionConstraint_Suspend();
    static void ExecutionConstraint_Resume ();

    CLR_UINT32 PerformGarbageCollection();
    void       PerformHeapCompaction   ();

    void Relocate();

    HRESULT ScheduleThreads( int maxContextSwitch );

    CLR_UINT32 WaitForActivity( CLR_UINT32 powerLevel, CLR_UINT32 events, CLR_INT64 timeout_ms );
    CLR_UINT32 WaitForActivity(                                                                );

#if defined(TINYCLR_JITTER)
    HRESULT Compile( const CLR_RT_MethodDef_Index& md, CLR_UINT32 flags );
#endif

#if defined(TINYCLR_JITTER_ARMEMULATION)
    HRESULT Emulate( CLR_RT_StackFrame* stack );
#endif

    HRESULT Execute      ( LPWSTR entryPointArgs, int maxContextSwitch  );

    HRESULT WaitForDebugger();

    static CLR_RT_HeapBlock* AccessStaticField( const CLR_RT_FieldDef_Index& fd );

    void ProcessTimeEvent( CLR_UINT32 event );

    static void InvalidateTimerCache();

    static CLR_INT64 GetUptime();

    CLR_RT_HeapBlock*      ExtractHeapBlocksForArray( CLR_RT_TypeDef_Instance& inst, CLR_UINT32 length, const CLR_RT_ReflectionDef_Index& reflex );
    CLR_RT_HeapBlock*      ExtractHeapBlocksForClassOrValueTypes( CLR_UINT32 dataType, CLR_UINT32 flags, const CLR_RT_TypeDef_Index& cls, CLR_UINT32 length );
    CLR_RT_HeapBlock*      ExtractHeapBytesForObjects           ( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 length );
    CLR_RT_HeapBlock*      ExtractHeapBlocksForObjects          ( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 length );
    CLR_RT_HeapBlock_Node* ExtractHeapBlocksForEvents           ( CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 length );

    HRESULT   NewThread      ( CLR_RT_Thread*& th, CLR_RT_HeapBlock_Delegate* pDelegate, int priority, CLR_INT32 id, CLR_UINT32 flags = 0 );
    void      PutInProperList( CLR_RT_Thread*  th                                                                                         );
    CLR_INT32 GetNextThreadId(                                                                                                            );

    HRESULT InitializeReference( CLR_RT_HeapBlock& ref, CLR_RT_SignatureParser&    parser                        );
    HRESULT InitializeReference( CLR_RT_HeapBlock& ref, const CLR_RECORD_FIELDDEF* target, CLR_RT_Assembly* assm );

    HRESULT InitializeLocals( CLR_RT_HeapBlock* locals, CLR_RT_Assembly* assm, const CLR_RECORD_METHODDEF* md );

    HRESULT NewObjectFromIndex( CLR_RT_HeapBlock& reference, const CLR_RT_TypeDef_Index&    cls                          );
    HRESULT NewObject         ( CLR_RT_HeapBlock& reference, const CLR_RT_TypeDef_Instance& inst                         );
    HRESULT NewObject         ( CLR_RT_HeapBlock& reference, CLR_UINT32                     token, CLR_RT_Assembly* assm );

    HRESULT CloneObject  ( CLR_RT_HeapBlock& reference  , const CLR_RT_HeapBlock& source );
    HRESULT CopyValueType( CLR_RT_HeapBlock* destination, const CLR_RT_HeapBlock* source );

    HRESULT NewArrayList ( CLR_RT_HeapBlock& ref, int size, CLR_RT_HeapBlock_Array*& array );


    HRESULT FindFieldDef( CLR_RT_TypeDef_Instance& inst     , LPCSTR szText                          , CLR_RT_FieldDef_Index& res );
    HRESULT FindFieldDef( CLR_RT_HeapBlock&        reference, LPCSTR szText                          , CLR_RT_FieldDef_Index& res );
    HRESULT FindField   ( CLR_RT_HeapBlock&        reference, LPCSTR szText, CLR_RT_HeapBlock*& field                             );
    HRESULT SetField    ( CLR_RT_HeapBlock&        reference, LPCSTR szText, CLR_RT_HeapBlock&  value                             );
    HRESULT GetField    ( CLR_RT_HeapBlock&        reference, LPCSTR szText, CLR_RT_HeapBlock&  value                             );


    HRESULT LockObject        ( CLR_RT_HeapBlock& reference, CLR_RT_SubThread* sth, const CLR_INT64& timeExpire, bool fForce );
    HRESULT UnlockObject      ( CLR_RT_HeapBlock& reference, CLR_RT_SubThread* sth                                           );
    void    DeleteLockRequests( CLR_RT_Thread*    thTarget , CLR_RT_SubThread* sthTarget                                     );

    HRESULT Sleep( CLR_RT_Thread* caller, const CLR_INT64& timeExpire );

    HRESULT WaitEvents  (                                CLR_RT_Thread* caller, const CLR_INT64& timeExpire, CLR_UINT32 events, bool& fSuccess );
    void    SignalEvents( CLR_RT_DblLinkedList& threads,                                                     CLR_UINT32 events                 );
    void    SignalEvents(                                                                                    CLR_UINT32 events                 );


    HRESULT InitTimeout( CLR_INT64& timeExpire, const CLR_INT64& timeout );
    HRESULT InitTimeout( CLR_INT64& timeExpire,       CLR_INT32  timeout );

    static bool IsInstanceOf( CLR_RT_TypeDescriptor&      desc, CLR_RT_TypeDescriptor& descTarget       );
    static bool IsInstanceOf( const CLR_RT_TypeDef_Index& cls , const CLR_RT_TypeDef_Index& clsTarget   );
    static bool IsInstanceOf( CLR_RT_HeapBlock&           obj , const CLR_RT_TypeDef_Index& clsTarget   );
    static bool IsInstanceOf( CLR_RT_HeapBlock&           obj , CLR_RT_Assembly* assm, CLR_UINT32 token );

    static HRESULT CastToType( CLR_RT_HeapBlock& ref, CLR_UINT32 tk, CLR_RT_Assembly* assm, bool fUpdate );

    void DebuggerLoop();

#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)
    void SetDebuggingInfoBreakpoints( bool fSet                                                                                                                                  );
    void InstallBreakpoints         ( CLR_DBG_Commands::Debugging_Execution_BreakpointDef* data, size_t num                                                                      );
    void StopOnBreakpoint           ( CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def , CLR_RT_Thread* th                                                               );
    void StopOnBreakpoint           ( CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def , CLR_RT_StackFrame* stack, CLR_PMETADATA ip                                      );
    void Breakpoint_System_Event    ( CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def , CLR_UINT16 event, CLR_RT_Thread* th, CLR_RT_StackFrame* stack, CLR_PMETADATA ip );
    bool DequeueActiveBreakpoint    ( CLR_DBG_Commands::Debugging_Execution_BreakpointDef& def                                                                                   );

    void Breakpoint_Assemblies_Loaded();

    void Breakpoint_Threads_Prepare  ( CLR_RT_DblLinkedList& threads );
    void Breakpoint_Thread_Terminated( CLR_RT_Thread*        th      );
    void Breakpoint_Thread_Created   ( CLR_RT_Thread*        th      );

    void Breakpoint_StackFrame_Break ( CLR_RT_StackFrame* stack                    );
    void Breakpoint_StackFrame_Push  ( CLR_RT_StackFrame* stack, CLR_UINT32 reason );
    void Breakpoint_StackFrame_Pop   ( CLR_RT_StackFrame* stack, bool stepEH       );
    void Breakpoint_StackFrame_Step  ( CLR_RT_StackFrame* stack, CLR_PMETADATA ip  );
    void Breakpoint_StackFrame_Hard  ( CLR_RT_StackFrame* stack, CLR_PMETADATA ip  );

    void Breakpoint_Exception            ( CLR_RT_StackFrame* stack, CLR_UINT32 reason, CLR_PMETADATA ip );
    void Breakpoint_Exception_Intercepted( CLR_RT_StackFrame* stack                                      );
    void Breakpoint_Exception_Uncaught   ( CLR_RT_Thread*     th                                         );
#endif //#if defined(TINYCLR_ENABLE_SOURCELEVELDEBUGGING)

    //--//

    bool IsTimeExpired( const CLR_INT64& timeExpire, CLR_INT64& timeoutMin, bool fAbsolute );

    bool IsThereEnoughIdleTime( CLR_UINT32 expectedMsec );

    void SpawnTimer();
    void SpawnFinalizer();
    void SpawnStaticConstructor( CLR_RT_Thread *&pCctorThread );
#if defined(TINYCLR_APPDOMAINS)
    bool SpawnStaticConstructorHelper( CLR_RT_AppDomain* appDomain, CLR_RT_AppDomainAssembly* appDomainAssembly, const CLR_RT_MethodDef_Index& idx );
#else
    bool SpawnStaticConstructorHelper( CLR_RT_Assembly* assembly, const CLR_RT_MethodDef_Index& idx );
#endif
    static void FinalizerTerminationCallback( void* arg );
    static void StaticConstructorTerminationCallback( void* arg );
    bool EnsureSystemThread( CLR_RT_Thread*& thread, int priority );
    
    CLR_INT64 ProcessTimer();
    
    //--//

    PROHIBIT_COPY_CONSTRUCTORS(CLR_RT_ExecutionEngine);

    //--//

private:

    HRESULT AllocateHeaps();

    void DeleteLockRequests( CLR_RT_Thread* thTarget, CLR_RT_SubThread* sthTarget, CLR_RT_DblLinkedList& threads );

    CLR_RT_HeapBlock* ExtractHeapBlocks ( CLR_RT_DblLinkedList& heap, CLR_UINT32 dataType, CLR_UINT32 flags, CLR_UINT32 length );

    CLR_RT_HeapBlock_Lock* FindLockObject( CLR_RT_DblLinkedList& threads, CLR_RT_HeapBlock& object );
    CLR_RT_HeapBlock_Lock* FindLockObject(                                CLR_RT_HeapBlock& object );

    void      CheckTimers    ( CLR_INT64& timeoutMin                                );
    void      CheckThreads   ( CLR_INT64& timeoutMin, CLR_RT_DblLinkedList& threads );

    void      ProcessHardware();


    void ReleaseAllThreads( CLR_RT_DblLinkedList& threads );
    void AbortAllThreads  ( CLR_RT_DblLinkedList& threads );

    void InsertThreadRoundRobin( CLR_RT_DblLinkedList& threads, CLR_RT_Thread* th );

    void UpdateTime      ( );
    
    CLR_UINT32 WaitSystemEvents( CLR_UINT32 powerLevel, CLR_UINT32 events, CLR_INT64 timeExpire );

#if defined(PLATFORM_WINDOWS) || defined(PLATFORM_WINCE)
    HRESULT CreateEntryPointArgs( CLR_RT_HeapBlock& args, LPWSTR szCommandLineArgs );
#endif

    // The lowest value for execution counter in threads.
    int                                 m_GlobalExecutionCounter;

    // Increase the value of m_executionCounter for all threads by iUpdateValue.
    // This ensures that counter does not underflow.
    void AdjustExecutionCounter( CLR_RT_DblLinkedList &threadList, int iUpdateValue );
public :
    
    // This function updates execution counter in pThread and makes it last to execute.
    // It is used to Thread.Sleep(0) imlementation. The thread is still ready, but is last to execute.
    void UpdateToLowestExecutionCounter( CLR_RT_Thread *pThread ) const;
    
    void RetrieveCurrentMethod( CLR_UINT32& assmIdx, CLR_UINT32& methodIdx );
};

extern CLR_RT_ExecutionEngine g_CLR_RT_ExecutionEngine;
extern CLR_UINT32             g_buildCRC;

//--//

//
// CT_ASSERT macro generates a compiler error in case the size of any structure changes.
//
CT_ASSERT( sizeof(CLR_RT_HeapBlock)      == 12 )
CT_ASSERT( sizeof(CLR_RT_HeapBlock_Raw)  == sizeof(CLR_RT_HeapBlock) )

#if defined(TINYCLR_TRACE_MEMORY_STATS)
#define    TINYCLR_TRACE_MEMORY_STATS_EXTRA_SIZE  4
#else
#define    TINYCLR_TRACE_MEMORY_STATS_EXTRA_SIZE  0
#endif

#if defined(ARM_V3_0) || defined(ARM_V3_1) // arm 3.0 compiler uses 8 bytes for a function pointer
    CT_ASSERT( sizeof(CLR_RT_DataTypeLookup) == 20 + TINYCLR_TRACE_MEMORY_STATS_EXTRA_SIZE )

#elif defined(ARM_V1_2)
    CT_ASSERT( sizeof(CLR_RT_DataTypeLookup) == 16 + TINYCLR_TRACE_MEMORY_STATS_EXTRA_SIZE )

#elif defined(GCC)  // Gcc compiler uses 8 bytes for a function pointer
    CT_ASSERT( sizeof(CLR_RT_DataTypeLookup) == 20 + TINYCLR_TRACE_MEMORY_STATS_EXTRA_SIZE )

#elif defined(GCCOP)  // GccOP compiler uses 8 bytes for a function pointer
    CT_ASSERT( sizeof(CLR_RT_DataTypeLookup) == 20 + TINYCLR_TRACE_MEMORY_STATS_EXTRA_SIZE )

#elif defined(PLATFORM_BLACKFIN) // 8 bytes for function pointer
    CT_ASSERT( sizeof(CLR_RT_DataTypeLookup) == 20 + TINYCLR_TRACE_MEMORY_STATS_EXTRA_SIZE )

#elif defined(PLATFORM_SH)  // SH.compiler uses 12 bytes for a function pointer
    CT_ASSERT( sizeof(CLR_RT_DataTypeLookup) == 24 + TINYCLR_TRACE_MEMORY_STATS_EXTRA_SIZE )

#elif defined(PLATFORM_WINDOWS) || defined(TINYCLR_TRACE_MEMORY_STATS) 
    CT_ASSERT( sizeof(CLR_RT_DataTypeLookup) == 16 + 4 )

#elif defined(PLATFORM_WINCE)
    CT_ASSERT( sizeof(CLR_RT_DataTypeLookup) == 16     )


#else

! ERROR

#endif

//--//

#if defined(PLATFORM_WINDOWS) || defined(PLATFORM_WINCE)
#pragma pack(pop, TINYCLR_RUNTIME_H)
#endif

#endif // _TINYCLR_RUNTIME_H_
