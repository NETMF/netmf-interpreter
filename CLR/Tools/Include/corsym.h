

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 6.00.0361 */
/* Compiler settings for corsym.idl:
    Oicf, W0, Zp8, env=Win32 (32b run)
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
//@@MIDL_FILE_HEADING(  )

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__

#ifndef COM_NO_WINDOWS_H
#include "windows.h"
#include "ole2.h"
#endif /*COM_NO_WINDOWS_H*/

#ifndef __corsym_h__
#define __corsym_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __CorSymWriter_deprecated_FWD_DEFINED__
#define __CorSymWriter_deprecated_FWD_DEFINED__

#ifdef __cplusplus
typedef class CorSymWriter_deprecated CorSymWriter_deprecated;
#else
typedef struct CorSymWriter_deprecated CorSymWriter_deprecated;
#endif /* __cplusplus */

#endif 	/* __CorSymWriter_deprecated_FWD_DEFINED__ */


#ifndef __CorSymReader_deprecated_FWD_DEFINED__
#define __CorSymReader_deprecated_FWD_DEFINED__

#ifdef __cplusplus
typedef class CorSymReader_deprecated CorSymReader_deprecated;
#else
typedef struct CorSymReader_deprecated CorSymReader_deprecated;
#endif /* __cplusplus */

#endif 	/* __CorSymReader_deprecated_FWD_DEFINED__ */


#ifndef __CorSymBinder_deprecated_FWD_DEFINED__
#define __CorSymBinder_deprecated_FWD_DEFINED__

#ifdef __cplusplus
typedef class CorSymBinder_deprecated CorSymBinder_deprecated;
#else
typedef struct CorSymBinder_deprecated CorSymBinder_deprecated;
#endif /* __cplusplus */

#endif 	/* __CorSymBinder_deprecated_FWD_DEFINED__ */


#ifndef __CorSymWriter_SxS_FWD_DEFINED__
#define __CorSymWriter_SxS_FWD_DEFINED__

#ifdef __cplusplus
typedef class CorSymWriter_SxS CorSymWriter_SxS;
#else
typedef struct CorSymWriter_SxS CorSymWriter_SxS;
#endif /* __cplusplus */

#endif 	/* __CorSymWriter_SxS_FWD_DEFINED__ */


#ifndef __CorSymReader_SxS_FWD_DEFINED__
#define __CorSymReader_SxS_FWD_DEFINED__

#ifdef __cplusplus
typedef class CorSymReader_SxS CorSymReader_SxS;
#else
typedef struct CorSymReader_SxS CorSymReader_SxS;
#endif /* __cplusplus */

#endif 	/* __CorSymReader_SxS_FWD_DEFINED__ */


#ifndef __CorSymBinder_SxS_FWD_DEFINED__
#define __CorSymBinder_SxS_FWD_DEFINED__

#ifdef __cplusplus
typedef class CorSymBinder_SxS CorSymBinder_SxS;
#else
typedef struct CorSymBinder_SxS CorSymBinder_SxS;
#endif /* __cplusplus */

#endif 	/* __CorSymBinder_SxS_FWD_DEFINED__ */


#ifndef __ISymUnmanagedBinder_FWD_DEFINED__
#define __ISymUnmanagedBinder_FWD_DEFINED__
typedef interface ISymUnmanagedBinder ISymUnmanagedBinder;
#endif 	/* __ISymUnmanagedBinder_FWD_DEFINED__ */


#ifndef __ISymUnmanagedBinder2_FWD_DEFINED__
#define __ISymUnmanagedBinder2_FWD_DEFINED__
typedef interface ISymUnmanagedBinder2 ISymUnmanagedBinder2;
#endif 	/* __ISymUnmanagedBinder2_FWD_DEFINED__ */


#ifndef __ISymUnmanagedBinder3_FWD_DEFINED__
#define __ISymUnmanagedBinder3_FWD_DEFINED__
typedef interface ISymUnmanagedBinder3 ISymUnmanagedBinder3;
#endif 	/* __ISymUnmanagedBinder3_FWD_DEFINED__ */


#ifndef __ISymUnmanagedDispose_FWD_DEFINED__
#define __ISymUnmanagedDispose_FWD_DEFINED__
typedef interface ISymUnmanagedDispose ISymUnmanagedDispose;
#endif 	/* __ISymUnmanagedDispose_FWD_DEFINED__ */


#ifndef __ISymUnmanagedDocument_FWD_DEFINED__
#define __ISymUnmanagedDocument_FWD_DEFINED__
typedef interface ISymUnmanagedDocument ISymUnmanagedDocument;
#endif 	/* __ISymUnmanagedDocument_FWD_DEFINED__ */


#ifndef __ISymUnmanagedDocumentWriter_FWD_DEFINED__
#define __ISymUnmanagedDocumentWriter_FWD_DEFINED__
typedef interface ISymUnmanagedDocumentWriter ISymUnmanagedDocumentWriter;
#endif 	/* __ISymUnmanagedDocumentWriter_FWD_DEFINED__ */


#ifndef __ISymUnmanagedMethod_FWD_DEFINED__
#define __ISymUnmanagedMethod_FWD_DEFINED__
typedef interface ISymUnmanagedMethod ISymUnmanagedMethod;
#endif 	/* __ISymUnmanagedMethod_FWD_DEFINED__ */


#ifndef __ISymENCUnmanagedMethod_FWD_DEFINED__
#define __ISymENCUnmanagedMethod_FWD_DEFINED__
typedef interface ISymENCUnmanagedMethod ISymENCUnmanagedMethod;
#endif 	/* __ISymENCUnmanagedMethod_FWD_DEFINED__ */


#ifndef __ISymUnmanagedNamespace_FWD_DEFINED__
#define __ISymUnmanagedNamespace_FWD_DEFINED__
typedef interface ISymUnmanagedNamespace ISymUnmanagedNamespace;
#endif 	/* __ISymUnmanagedNamespace_FWD_DEFINED__ */


#ifndef __ISymUnmanagedReader_FWD_DEFINED__
#define __ISymUnmanagedReader_FWD_DEFINED__
typedef interface ISymUnmanagedReader ISymUnmanagedReader;
#endif 	/* __ISymUnmanagedReader_FWD_DEFINED__ */


#ifndef __ISymUnmanagedENCUpdate_FWD_DEFINED__
#define __ISymUnmanagedENCUpdate_FWD_DEFINED__
typedef interface ISymUnmanagedENCUpdate ISymUnmanagedENCUpdate;
#endif 	/* __ISymUnmanagedENCUpdate_FWD_DEFINED__ */


#ifndef __ISymUnmanagedReaderSymbolSearchInfo_FWD_DEFINED__
#define __ISymUnmanagedReaderSymbolSearchInfo_FWD_DEFINED__
typedef interface ISymUnmanagedReaderSymbolSearchInfo ISymUnmanagedReaderSymbolSearchInfo;
#endif 	/* __ISymUnmanagedReaderSymbolSearchInfo_FWD_DEFINED__ */


#ifndef __ISymUnmanagedScope_FWD_DEFINED__
#define __ISymUnmanagedScope_FWD_DEFINED__
typedef interface ISymUnmanagedScope ISymUnmanagedScope;
#endif 	/* __ISymUnmanagedScope_FWD_DEFINED__ */


#ifndef __ISymUnmanagedConstant_FWD_DEFINED__
#define __ISymUnmanagedConstant_FWD_DEFINED__
typedef interface ISymUnmanagedConstant ISymUnmanagedConstant;
#endif 	/* __ISymUnmanagedConstant_FWD_DEFINED__ */


#ifndef __ISymUnmanagedScope2_FWD_DEFINED__
#define __ISymUnmanagedScope2_FWD_DEFINED__
typedef interface ISymUnmanagedScope2 ISymUnmanagedScope2;
#endif 	/* __ISymUnmanagedScope2_FWD_DEFINED__ */


#ifndef __ISymUnmanagedVariable_FWD_DEFINED__
#define __ISymUnmanagedVariable_FWD_DEFINED__
typedef interface ISymUnmanagedVariable ISymUnmanagedVariable;
#endif 	/* __ISymUnmanagedVariable_FWD_DEFINED__ */


#ifndef __ISymUnmanagedSymbolSearchInfo_FWD_DEFINED__
#define __ISymUnmanagedSymbolSearchInfo_FWD_DEFINED__
typedef interface ISymUnmanagedSymbolSearchInfo ISymUnmanagedSymbolSearchInfo;
#endif 	/* __ISymUnmanagedSymbolSearchInfo_FWD_DEFINED__ */


#ifndef __ISymUnmanagedWriter_FWD_DEFINED__
#define __ISymUnmanagedWriter_FWD_DEFINED__
typedef interface ISymUnmanagedWriter ISymUnmanagedWriter;
#endif 	/* __ISymUnmanagedWriter_FWD_DEFINED__ */


#ifndef __ISymUnmanagedWriter2_FWD_DEFINED__
#define __ISymUnmanagedWriter2_FWD_DEFINED__
typedef interface ISymUnmanagedWriter2 ISymUnmanagedWriter2;
#endif 	/* __ISymUnmanagedWriter2_FWD_DEFINED__ */


#ifndef __ISymUnmanagedWriter3_FWD_DEFINED__
#define __ISymUnmanagedWriter3_FWD_DEFINED__
typedef interface ISymUnmanagedWriter3 ISymUnmanagedWriter3;
#endif 	/* __ISymUnmanagedWriter3_FWD_DEFINED__ */


#ifndef __ISymUnmanagedReader2_FWD_DEFINED__
#define __ISymUnmanagedReader2_FWD_DEFINED__
typedef interface ISymUnmanagedReader2 ISymUnmanagedReader2;
#endif 	/* __ISymUnmanagedReader2_FWD_DEFINED__ */


#ifdef __cplusplus
extern "C"{
#endif 

void * __RPC_USER MIDL_user_allocate(size_t);
void __RPC_USER MIDL_user_free( void * ); 

/* interface __MIDL_itf_corsym_0000 */
/* [local] */ 

#ifndef __CORHDR_H__
typedef mdToken mdSignature;

#endif
EXTERN_GUID(CorSym_LanguageType_C, 0x63a08714, 0xfc37, 0x11d2, 0x90, 0x4c, 0x0, 0xc0, 0x4f, 0xa3, 0x02, 0xa1);
EXTERN_GUID(CorSym_LanguageType_CPlusPlus, 0x3a12d0b7, 0xc26c, 0x11d0, 0xb4, 0x42, 0x0, 0xa0, 0x24, 0x4a, 0x1d, 0xd2);
EXTERN_GUID(CorSym_LanguageType_CSharp, 0x3f5162f8, 0x07c6, 0x11d3, 0x90, 0x53, 0x0, 0xc0, 0x4f, 0xa3, 0x02, 0xa1);
EXTERN_GUID(CorSym_LanguageType_Basic, 0x3a12d0b8, 0xc26c, 0x11d0, 0xb4, 0x42, 0x0, 0xa0, 0x24, 0x4a, 0x1d, 0xd2);
EXTERN_GUID(CorSym_LanguageType_Java, 0x3a12d0b4, 0xc26c, 0x11d0, 0xb4, 0x42, 0x0, 0xa0, 0x24, 0x4a, 0x1d, 0xd2);
EXTERN_GUID(CorSym_LanguageType_Cobol, 0xaf046cd1, 0xd0e1, 0x11d2, 0x97, 0x7c, 0x0, 0xa0, 0xc9, 0xb4, 0xd5, 0xc);
EXTERN_GUID(CorSym_LanguageType_Pascal, 0xaf046cd2, 0xd0e1, 0x11d2, 0x97, 0x7c, 0x0, 0xa0, 0xc9, 0xb4, 0xd5, 0xc);
EXTERN_GUID(CorSym_LanguageType_ILAssembly, 0xaf046cd3, 0xd0e1, 0x11d2, 0x97, 0x7c, 0x0, 0xa0, 0xc9, 0xb4, 0xd5, 0xc);
EXTERN_GUID(CorSym_LanguageType_JScript, 0x3a12d0b6, 0xc26c, 0x11d0, 0xb4, 0x42, 0x00, 0xa0, 0x24, 0x4a, 0x1d, 0xd2);
EXTERN_GUID(CorSym_LanguageType_SMC, 0xd9b9f7b, 0x6611, 0x11d3, 0xbd, 0x2a, 0x0, 0x0, 0xf8, 0x8, 0x49, 0xbd);
EXTERN_GUID(CorSym_LanguageType_MCPlusPlus, 0x4b35fde8, 0x07c6, 0x11d3, 0x90, 0x53, 0x0, 0xc0, 0x4f, 0xa3, 0x02, 0xa1);
EXTERN_GUID(CorSym_LanguageVendor_Microsoft, 0x994b45c4, 0xe6e9, 0x11d2, 0x90, 0x3f, 0x00, 0xc0, 0x4f, 0xa3, 0x02, 0xa1);
EXTERN_GUID(CorSym_DocumentType_Text, 0x5a869d0b, 0x6611, 0x11d3, 0xbd, 0x2a, 0x0, 0x0, 0xf8, 0x8, 0x49, 0xbd);
EXTERN_GUID(CorSym_DocumentType_MC, 0xeb40cb65, 0x3c1f, 0x4352, 0x9d, 0x7b, 0xba, 0xf, 0xc4, 0x7a, 0x9d, 0x77);


typedef 
enum CorSymAddrKind
    {	ADDR_IL_OFFSET	= 1,
	ADDR_NATIVE_RVA	= 2,
	ADDR_NATIVE_REGISTER	= 3,
	ADDR_NATIVE_REGREL	= 4,
	ADDR_NATIVE_OFFSET	= 5,
	ADDR_NATIVE_REGREG	= 6,
	ADDR_NATIVE_REGSTK	= 7,
	ADDR_NATIVE_STKREG	= 8,
	ADDR_BITFIELD	= 9,
	ADDR_NATIVE_ISECTOFFSET	= 10
    } 	CorSymAddrKind;

typedef 
enum CorSymVarFlag
    {	VAR_IS_COMP_GEN	= 1
    } 	CorSymVarFlag;



extern RPC_IF_HANDLE __MIDL_itf_corsym_0000_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_corsym_0000_v0_0_s_ifspec;


#ifndef __CorSymLib_LIBRARY_DEFINED__
#define __CorSymLib_LIBRARY_DEFINED__

/* library CorSymLib */
/* [helpstring][version][uuid] */ 


EXTERN_C const IID LIBID_CorSymLib;

EXTERN_C const CLSID CLSID_CorSymWriter_deprecated;

#ifdef __cplusplus

class DECLSPEC_UUID("108296C1-281E-11d3-BD22-0000F80849BD")
CorSymWriter_deprecated;
#endif

EXTERN_C const CLSID CLSID_CorSymReader_deprecated;

#ifdef __cplusplus

class DECLSPEC_UUID("108296C2-281E-11d3-BD22-0000F80849BD")
CorSymReader_deprecated;
#endif

EXTERN_C const CLSID CLSID_CorSymBinder_deprecated;

#ifdef __cplusplus

class DECLSPEC_UUID("AA544D41-28CB-11d3-BD22-0000F80849BD")
CorSymBinder_deprecated;
#endif

EXTERN_C const CLSID CLSID_CorSymWriter_SxS;

#ifdef __cplusplus

class DECLSPEC_UUID("0AE2DEB0-F901-478b-BB9F-881EE8066788")
CorSymWriter_SxS;
#endif

EXTERN_C const CLSID CLSID_CorSymReader_SxS;

#ifdef __cplusplus

class DECLSPEC_UUID("0A3976C5-4529-4ef8-B0B0-42EED37082CD")
CorSymReader_SxS;
#endif

EXTERN_C const CLSID CLSID_CorSymBinder_SxS;

#ifdef __cplusplus

class DECLSPEC_UUID("0A29FF9E-7F9C-4437-8B11-F424491E3931")
CorSymBinder_SxS;
#endif
#endif /* __CorSymLib_LIBRARY_DEFINED__ */

#ifndef __ISymUnmanagedBinder_INTERFACE_DEFINED__
#define __ISymUnmanagedBinder_INTERFACE_DEFINED__

/* interface ISymUnmanagedBinder */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedBinder;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("AA544D42-28CB-11d3-BD22-0000F80849BD")
    ISymUnmanagedBinder : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetReaderForFile( 
            /* [in] */ IUnknown *importer,
            /* [in] */ const WCHAR *fileName,
            /* [in] */ const WCHAR *searchPath,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetReaderFromStream( 
            /* [in] */ IUnknown *importer,
            /* [in] */ IStream *pstream,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedBinderVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedBinder * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedBinder * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedBinder * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetReaderForFile )( 
            ISymUnmanagedBinder * This,
            /* [in] */ IUnknown *importer,
            /* [in] */ const WCHAR *fileName,
            /* [in] */ const WCHAR *searchPath,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetReaderFromStream )( 
            ISymUnmanagedBinder * This,
            /* [in] */ IUnknown *importer,
            /* [in] */ IStream *pstream,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal);
        
        END_INTERFACE
    } ISymUnmanagedBinderVtbl;

    interface ISymUnmanagedBinder
    {
        CONST_VTBL struct ISymUnmanagedBinderVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedBinder_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedBinder_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedBinder_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedBinder_GetReaderForFile(This,importer,fileName,searchPath,pRetVal)	\
    (This)->lpVtbl -> GetReaderForFile(This,importer,fileName,searchPath,pRetVal)

#define ISymUnmanagedBinder_GetReaderFromStream(This,importer,pstream,pRetVal)	\
    (This)->lpVtbl -> GetReaderFromStream(This,importer,pstream,pRetVal)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedBinder_GetReaderForFile_Proxy( 
    ISymUnmanagedBinder * This,
    /* [in] */ IUnknown *importer,
    /* [in] */ const WCHAR *fileName,
    /* [in] */ const WCHAR *searchPath,
    /* [retval][out] */ ISymUnmanagedReader **pRetVal);


void __RPC_STUB ISymUnmanagedBinder_GetReaderForFile_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedBinder_GetReaderFromStream_Proxy( 
    ISymUnmanagedBinder * This,
    /* [in] */ IUnknown *importer,
    /* [in] */ IStream *pstream,
    /* [retval][out] */ ISymUnmanagedReader **pRetVal);


void __RPC_STUB ISymUnmanagedBinder_GetReaderFromStream_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedBinder_INTERFACE_DEFINED__ */


/* interface __MIDL_itf_corsym_0115 */
/* [local] */ 

typedef 
enum CorSymSearchPolicyAttributes
    {	AllowRegistryAccess	= 0x1,
	AllowSymbolServerAccess	= 0x2,
	AllowOriginalPathAccess	= 0x4,
	AllowReferencePathAccess	= 0x8
    } 	CorSymSearchPolicyAttributes;



extern RPC_IF_HANDLE __MIDL_itf_corsym_0115_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_corsym_0115_v0_0_s_ifspec;

#ifndef __ISymUnmanagedBinder2_INTERFACE_DEFINED__
#define __ISymUnmanagedBinder2_INTERFACE_DEFINED__

/* interface ISymUnmanagedBinder2 */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedBinder2;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("ACCEE350-89AF-4ccb-8B40-1C2C4C6F9434")
    ISymUnmanagedBinder2 : public ISymUnmanagedBinder
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetReaderForFile2( 
            /* [in] */ IUnknown *importer,
            /* [in] */ const WCHAR *fileName,
            /* [in] */ const WCHAR *searchPath,
            /* [in] */ ULONG32 searchPolicy,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetReaderForFile3( 
            /* [in] */ IUnknown *importer,
            /* [in] */ const WCHAR *fileName,
            /* [in] */ const WCHAR *searchPath,
            /* [in] */ ULONG32 searchPolicy,
            /* [in] */ IUnknown *callback,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedBinder2Vtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedBinder2 * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedBinder2 * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedBinder2 * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetReaderForFile )( 
            ISymUnmanagedBinder2 * This,
            /* [in] */ IUnknown *importer,
            /* [in] */ const WCHAR *fileName,
            /* [in] */ const WCHAR *searchPath,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetReaderFromStream )( 
            ISymUnmanagedBinder2 * This,
            /* [in] */ IUnknown *importer,
            /* [in] */ IStream *pstream,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetReaderForFile2 )( 
            ISymUnmanagedBinder2 * This,
            /* [in] */ IUnknown *importer,
            /* [in] */ const WCHAR *fileName,
            /* [in] */ const WCHAR *searchPath,
            /* [in] */ ULONG32 searchPolicy,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetReaderForFile3 )( 
            ISymUnmanagedBinder2 * This,
            /* [in] */ IUnknown *importer,
            /* [in] */ const WCHAR *fileName,
            /* [in] */ const WCHAR *searchPath,
            /* [in] */ ULONG32 searchPolicy,
            /* [in] */ IUnknown *callback,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal);
        
        END_INTERFACE
    } ISymUnmanagedBinder2Vtbl;

    interface ISymUnmanagedBinder2
    {
        CONST_VTBL struct ISymUnmanagedBinder2Vtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedBinder2_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedBinder2_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedBinder2_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedBinder2_GetReaderForFile(This,importer,fileName,searchPath,pRetVal)	\
    (This)->lpVtbl -> GetReaderForFile(This,importer,fileName,searchPath,pRetVal)

#define ISymUnmanagedBinder2_GetReaderFromStream(This,importer,pstream,pRetVal)	\
    (This)->lpVtbl -> GetReaderFromStream(This,importer,pstream,pRetVal)


#define ISymUnmanagedBinder2_GetReaderForFile2(This,importer,fileName,searchPath,searchPolicy,pRetVal)	\
    (This)->lpVtbl -> GetReaderForFile2(This,importer,fileName,searchPath,searchPolicy,pRetVal)

#define ISymUnmanagedBinder2_GetReaderForFile3(This,importer,fileName,searchPath,searchPolicy,callback,pRetVal)	\
    (This)->lpVtbl -> GetReaderForFile3(This,importer,fileName,searchPath,searchPolicy,callback,pRetVal)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedBinder2_GetReaderForFile2_Proxy( 
    ISymUnmanagedBinder2 * This,
    /* [in] */ IUnknown *importer,
    /* [in] */ const WCHAR *fileName,
    /* [in] */ const WCHAR *searchPath,
    /* [in] */ ULONG32 searchPolicy,
    /* [retval][out] */ ISymUnmanagedReader **pRetVal);


void __RPC_STUB ISymUnmanagedBinder2_GetReaderForFile2_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedBinder2_GetReaderForFile3_Proxy( 
    ISymUnmanagedBinder2 * This,
    /* [in] */ IUnknown *importer,
    /* [in] */ const WCHAR *fileName,
    /* [in] */ const WCHAR *searchPath,
    /* [in] */ ULONG32 searchPolicy,
    /* [in] */ IUnknown *callback,
    /* [retval][out] */ ISymUnmanagedReader **pRetVal);


void __RPC_STUB ISymUnmanagedBinder2_GetReaderForFile3_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedBinder2_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedBinder3_INTERFACE_DEFINED__
#define __ISymUnmanagedBinder3_INTERFACE_DEFINED__

/* interface ISymUnmanagedBinder3 */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedBinder3;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("28AD3D43-B601-4d26-8A1B-25F9165AF9D7")
    ISymUnmanagedBinder3 : public ISymUnmanagedBinder2
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetReaderFromCallback( 
            /* [in] */ IUnknown *importer,
            /* [in] */ const WCHAR *fileName,
            /* [in] */ const WCHAR *searchPath,
            /* [in] */ ULONG32 searchPolicy,
            /* [in] */ IUnknown *callback,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedBinder3Vtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedBinder3 * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedBinder3 * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedBinder3 * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetReaderForFile )( 
            ISymUnmanagedBinder3 * This,
            /* [in] */ IUnknown *importer,
            /* [in] */ const WCHAR *fileName,
            /* [in] */ const WCHAR *searchPath,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetReaderFromStream )( 
            ISymUnmanagedBinder3 * This,
            /* [in] */ IUnknown *importer,
            /* [in] */ IStream *pstream,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetReaderForFile2 )( 
            ISymUnmanagedBinder3 * This,
            /* [in] */ IUnknown *importer,
            /* [in] */ const WCHAR *fileName,
            /* [in] */ const WCHAR *searchPath,
            /* [in] */ ULONG32 searchPolicy,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetReaderForFile3 )( 
            ISymUnmanagedBinder3 * This,
            /* [in] */ IUnknown *importer,
            /* [in] */ const WCHAR *fileName,
            /* [in] */ const WCHAR *searchPath,
            /* [in] */ ULONG32 searchPolicy,
            /* [in] */ IUnknown *callback,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetReaderFromCallback )( 
            ISymUnmanagedBinder3 * This,
            /* [in] */ IUnknown *importer,
            /* [in] */ const WCHAR *fileName,
            /* [in] */ const WCHAR *searchPath,
            /* [in] */ ULONG32 searchPolicy,
            /* [in] */ IUnknown *callback,
            /* [retval][out] */ ISymUnmanagedReader **pRetVal);
        
        END_INTERFACE
    } ISymUnmanagedBinder3Vtbl;

    interface ISymUnmanagedBinder3
    {
        CONST_VTBL struct ISymUnmanagedBinder3Vtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedBinder3_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedBinder3_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedBinder3_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedBinder3_GetReaderForFile(This,importer,fileName,searchPath,pRetVal)	\
    (This)->lpVtbl -> GetReaderForFile(This,importer,fileName,searchPath,pRetVal)

#define ISymUnmanagedBinder3_GetReaderFromStream(This,importer,pstream,pRetVal)	\
    (This)->lpVtbl -> GetReaderFromStream(This,importer,pstream,pRetVal)


#define ISymUnmanagedBinder3_GetReaderForFile2(This,importer,fileName,searchPath,searchPolicy,pRetVal)	\
    (This)->lpVtbl -> GetReaderForFile2(This,importer,fileName,searchPath,searchPolicy,pRetVal)

#define ISymUnmanagedBinder3_GetReaderForFile3(This,importer,fileName,searchPath,searchPolicy,callback,pRetVal)	\
    (This)->lpVtbl -> GetReaderForFile3(This,importer,fileName,searchPath,searchPolicy,callback,pRetVal)


#define ISymUnmanagedBinder3_GetReaderFromCallback(This,importer,fileName,searchPath,searchPolicy,callback,pRetVal)	\
    (This)->lpVtbl -> GetReaderFromCallback(This,importer,fileName,searchPath,searchPolicy,callback,pRetVal)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedBinder3_GetReaderFromCallback_Proxy( 
    ISymUnmanagedBinder3 * This,
    /* [in] */ IUnknown *importer,
    /* [in] */ const WCHAR *fileName,
    /* [in] */ const WCHAR *searchPath,
    /* [in] */ ULONG32 searchPolicy,
    /* [in] */ IUnknown *callback,
    /* [retval][out] */ ISymUnmanagedReader **pRetVal);


void __RPC_STUB ISymUnmanagedBinder3_GetReaderFromCallback_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedBinder3_INTERFACE_DEFINED__ */


/* interface __MIDL_itf_corsym_0117 */
/* [local] */ 

static const int E_SYM_DESTROYED = MAKE_HRESULT(1, FACILITY_ITF, 0xdead);


extern RPC_IF_HANDLE __MIDL_itf_corsym_0117_v0_0_c_ifspec;
extern RPC_IF_HANDLE __MIDL_itf_corsym_0117_v0_0_s_ifspec;

#ifndef __ISymUnmanagedDispose_INTERFACE_DEFINED__
#define __ISymUnmanagedDispose_INTERFACE_DEFINED__

/* interface ISymUnmanagedDispose */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedDispose;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("969708D2-05E5-4861-A3B0-96E473CDF63F")
    ISymUnmanagedDispose : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE Destroy( void) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedDisposeVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedDispose * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedDispose * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedDispose * This);
        
        HRESULT ( STDMETHODCALLTYPE *Destroy )( 
            ISymUnmanagedDispose * This);
        
        END_INTERFACE
    } ISymUnmanagedDisposeVtbl;

    interface ISymUnmanagedDispose
    {
        CONST_VTBL struct ISymUnmanagedDisposeVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedDispose_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedDispose_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedDispose_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedDispose_Destroy(This)	\
    (This)->lpVtbl -> Destroy(This)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedDispose_Destroy_Proxy( 
    ISymUnmanagedDispose * This);


void __RPC_STUB ISymUnmanagedDispose_Destroy_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedDispose_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedDocument_INTERFACE_DEFINED__
#define __ISymUnmanagedDocument_INTERFACE_DEFINED__

/* interface ISymUnmanagedDocument */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedDocument;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("40DE4037-7C81-3E1E-B022-AE1ABFF2CA08")
    ISymUnmanagedDocument : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetURL( 
            /* [in] */ ULONG32 cchUrl,
            /* [out] */ ULONG32 *pcchUrl,
            /* [length_is][size_is][out] */ WCHAR szUrl[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetDocumentType( 
            /* [retval][out] */ GUID *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetLanguage( 
            /* [retval][out] */ GUID *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetLanguageVendor( 
            /* [retval][out] */ GUID *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetCheckSumAlgorithmId( 
            /* [retval][out] */ GUID *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetCheckSum( 
            /* [in] */ ULONG32 cData,
            /* [out] */ ULONG32 *pcData,
            /* [length_is][size_is][out] */ BYTE data[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE FindClosestLine( 
            /* [in] */ ULONG32 line,
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE HasEmbeddedSource( 
            /* [retval][out] */ BOOL *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetSourceLength( 
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetSourceRange( 
            /* [in] */ ULONG32 startLine,
            /* [in] */ ULONG32 startColumn,
            /* [in] */ ULONG32 endLine,
            /* [in] */ ULONG32 endColumn,
            /* [in] */ ULONG32 cSourceBytes,
            /* [out] */ ULONG32 *pcSourceBytes,
            /* [length_is][size_is][out] */ BYTE source[  ]) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedDocumentVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedDocument * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedDocument * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedDocument * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetURL )( 
            ISymUnmanagedDocument * This,
            /* [in] */ ULONG32 cchUrl,
            /* [out] */ ULONG32 *pcchUrl,
            /* [length_is][size_is][out] */ WCHAR szUrl[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetDocumentType )( 
            ISymUnmanagedDocument * This,
            /* [retval][out] */ GUID *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetLanguage )( 
            ISymUnmanagedDocument * This,
            /* [retval][out] */ GUID *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetLanguageVendor )( 
            ISymUnmanagedDocument * This,
            /* [retval][out] */ GUID *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetCheckSumAlgorithmId )( 
            ISymUnmanagedDocument * This,
            /* [retval][out] */ GUID *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetCheckSum )( 
            ISymUnmanagedDocument * This,
            /* [in] */ ULONG32 cData,
            /* [out] */ ULONG32 *pcData,
            /* [length_is][size_is][out] */ BYTE data[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *FindClosestLine )( 
            ISymUnmanagedDocument * This,
            /* [in] */ ULONG32 line,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *HasEmbeddedSource )( 
            ISymUnmanagedDocument * This,
            /* [retval][out] */ BOOL *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetSourceLength )( 
            ISymUnmanagedDocument * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetSourceRange )( 
            ISymUnmanagedDocument * This,
            /* [in] */ ULONG32 startLine,
            /* [in] */ ULONG32 startColumn,
            /* [in] */ ULONG32 endLine,
            /* [in] */ ULONG32 endColumn,
            /* [in] */ ULONG32 cSourceBytes,
            /* [out] */ ULONG32 *pcSourceBytes,
            /* [length_is][size_is][out] */ BYTE source[  ]);
        
        END_INTERFACE
    } ISymUnmanagedDocumentVtbl;

    interface ISymUnmanagedDocument
    {
        CONST_VTBL struct ISymUnmanagedDocumentVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedDocument_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedDocument_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedDocument_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedDocument_GetURL(This,cchUrl,pcchUrl,szUrl)	\
    (This)->lpVtbl -> GetURL(This,cchUrl,pcchUrl,szUrl)

#define ISymUnmanagedDocument_GetDocumentType(This,pRetVal)	\
    (This)->lpVtbl -> GetDocumentType(This,pRetVal)

#define ISymUnmanagedDocument_GetLanguage(This,pRetVal)	\
    (This)->lpVtbl -> GetLanguage(This,pRetVal)

#define ISymUnmanagedDocument_GetLanguageVendor(This,pRetVal)	\
    (This)->lpVtbl -> GetLanguageVendor(This,pRetVal)

#define ISymUnmanagedDocument_GetCheckSumAlgorithmId(This,pRetVal)	\
    (This)->lpVtbl -> GetCheckSumAlgorithmId(This,pRetVal)

#define ISymUnmanagedDocument_GetCheckSum(This,cData,pcData,data)	\
    (This)->lpVtbl -> GetCheckSum(This,cData,pcData,data)

#define ISymUnmanagedDocument_FindClosestLine(This,line,pRetVal)	\
    (This)->lpVtbl -> FindClosestLine(This,line,pRetVal)

#define ISymUnmanagedDocument_HasEmbeddedSource(This,pRetVal)	\
    (This)->lpVtbl -> HasEmbeddedSource(This,pRetVal)

#define ISymUnmanagedDocument_GetSourceLength(This,pRetVal)	\
    (This)->lpVtbl -> GetSourceLength(This,pRetVal)

#define ISymUnmanagedDocument_GetSourceRange(This,startLine,startColumn,endLine,endColumn,cSourceBytes,pcSourceBytes,source)	\
    (This)->lpVtbl -> GetSourceRange(This,startLine,startColumn,endLine,endColumn,cSourceBytes,pcSourceBytes,source)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedDocument_GetURL_Proxy( 
    ISymUnmanagedDocument * This,
    /* [in] */ ULONG32 cchUrl,
    /* [out] */ ULONG32 *pcchUrl,
    /* [length_is][size_is][out] */ WCHAR szUrl[  ]);


void __RPC_STUB ISymUnmanagedDocument_GetURL_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedDocument_GetDocumentType_Proxy( 
    ISymUnmanagedDocument * This,
    /* [retval][out] */ GUID *pRetVal);


void __RPC_STUB ISymUnmanagedDocument_GetDocumentType_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedDocument_GetLanguage_Proxy( 
    ISymUnmanagedDocument * This,
    /* [retval][out] */ GUID *pRetVal);


void __RPC_STUB ISymUnmanagedDocument_GetLanguage_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedDocument_GetLanguageVendor_Proxy( 
    ISymUnmanagedDocument * This,
    /* [retval][out] */ GUID *pRetVal);


void __RPC_STUB ISymUnmanagedDocument_GetLanguageVendor_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedDocument_GetCheckSumAlgorithmId_Proxy( 
    ISymUnmanagedDocument * This,
    /* [retval][out] */ GUID *pRetVal);


void __RPC_STUB ISymUnmanagedDocument_GetCheckSumAlgorithmId_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedDocument_GetCheckSum_Proxy( 
    ISymUnmanagedDocument * This,
    /* [in] */ ULONG32 cData,
    /* [out] */ ULONG32 *pcData,
    /* [length_is][size_is][out] */ BYTE data[  ]);


void __RPC_STUB ISymUnmanagedDocument_GetCheckSum_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedDocument_FindClosestLine_Proxy( 
    ISymUnmanagedDocument * This,
    /* [in] */ ULONG32 line,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedDocument_FindClosestLine_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedDocument_HasEmbeddedSource_Proxy( 
    ISymUnmanagedDocument * This,
    /* [retval][out] */ BOOL *pRetVal);


void __RPC_STUB ISymUnmanagedDocument_HasEmbeddedSource_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedDocument_GetSourceLength_Proxy( 
    ISymUnmanagedDocument * This,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedDocument_GetSourceLength_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedDocument_GetSourceRange_Proxy( 
    ISymUnmanagedDocument * This,
    /* [in] */ ULONG32 startLine,
    /* [in] */ ULONG32 startColumn,
    /* [in] */ ULONG32 endLine,
    /* [in] */ ULONG32 endColumn,
    /* [in] */ ULONG32 cSourceBytes,
    /* [out] */ ULONG32 *pcSourceBytes,
    /* [length_is][size_is][out] */ BYTE source[  ]);


void __RPC_STUB ISymUnmanagedDocument_GetSourceRange_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedDocument_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedDocumentWriter_INTERFACE_DEFINED__
#define __ISymUnmanagedDocumentWriter_INTERFACE_DEFINED__

/* interface ISymUnmanagedDocumentWriter */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedDocumentWriter;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("B01FAFEB-C450-3A4D-BEEC-B4CEEC01E006")
    ISymUnmanagedDocumentWriter : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE SetSource( 
            /* [in] */ ULONG32 sourceSize,
            /* [size_is][in] */ BYTE source[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE SetCheckSum( 
            /* [in] */ GUID algorithmId,
            /* [in] */ ULONG32 checkSumSize,
            /* [size_is][in] */ BYTE checkSum[  ]) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedDocumentWriterVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedDocumentWriter * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedDocumentWriter * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedDocumentWriter * This);
        
        HRESULT ( STDMETHODCALLTYPE *SetSource )( 
            ISymUnmanagedDocumentWriter * This,
            /* [in] */ ULONG32 sourceSize,
            /* [size_is][in] */ BYTE source[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *SetCheckSum )( 
            ISymUnmanagedDocumentWriter * This,
            /* [in] */ GUID algorithmId,
            /* [in] */ ULONG32 checkSumSize,
            /* [size_is][in] */ BYTE checkSum[  ]);
        
        END_INTERFACE
    } ISymUnmanagedDocumentWriterVtbl;

    interface ISymUnmanagedDocumentWriter
    {
        CONST_VTBL struct ISymUnmanagedDocumentWriterVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedDocumentWriter_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedDocumentWriter_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedDocumentWriter_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedDocumentWriter_SetSource(This,sourceSize,source)	\
    (This)->lpVtbl -> SetSource(This,sourceSize,source)

#define ISymUnmanagedDocumentWriter_SetCheckSum(This,algorithmId,checkSumSize,checkSum)	\
    (This)->lpVtbl -> SetCheckSum(This,algorithmId,checkSumSize,checkSum)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedDocumentWriter_SetSource_Proxy( 
    ISymUnmanagedDocumentWriter * This,
    /* [in] */ ULONG32 sourceSize,
    /* [size_is][in] */ BYTE source[  ]);


void __RPC_STUB ISymUnmanagedDocumentWriter_SetSource_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedDocumentWriter_SetCheckSum_Proxy( 
    ISymUnmanagedDocumentWriter * This,
    /* [in] */ GUID algorithmId,
    /* [in] */ ULONG32 checkSumSize,
    /* [size_is][in] */ BYTE checkSum[  ]);


void __RPC_STUB ISymUnmanagedDocumentWriter_SetCheckSum_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedDocumentWriter_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedMethod_INTERFACE_DEFINED__
#define __ISymUnmanagedMethod_INTERFACE_DEFINED__

/* interface ISymUnmanagedMethod */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedMethod;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("B62B923C-B500-3158-A543-24F307A8B7E1")
    ISymUnmanagedMethod : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetToken( 
            /* [retval][out] */ mdMethodDef *pToken) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetSequencePointCount( 
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetRootScope( 
            /* [retval][out] */ ISymUnmanagedScope **pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetScopeFromOffset( 
            /* [in] */ ULONG32 offset,
            /* [retval][out] */ ISymUnmanagedScope **pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetOffset( 
            /* [in] */ ISymUnmanagedDocument *document,
            /* [in] */ ULONG32 line,
            /* [in] */ ULONG32 column,
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetRanges( 
            /* [in] */ ISymUnmanagedDocument *document,
            /* [in] */ ULONG32 line,
            /* [in] */ ULONG32 column,
            /* [in] */ ULONG32 cRanges,
            /* [out] */ ULONG32 *pcRanges,
            /* [length_is][size_is][out] */ ULONG32 ranges[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetParameters( 
            /* [in] */ ULONG32 cParams,
            /* [out] */ ULONG32 *pcParams,
            /* [length_is][size_is][out] */ ISymUnmanagedVariable *params[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetNamespace( 
            /* [out] */ ISymUnmanagedNamespace **pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetSourceStartEnd( 
            /* [in] */ ISymUnmanagedDocument *docs[ 2 ],
            /* [in] */ ULONG32 lines[ 2 ],
            /* [in] */ ULONG32 columns[ 2 ],
            /* [out] */ BOOL *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetSequencePoints( 
            /* [in] */ ULONG32 cPoints,
            /* [out] */ ULONG32 *pcPoints,
            /* [size_is][in] */ ULONG32 offsets[  ],
            /* [size_is][in] */ ISymUnmanagedDocument *documents[  ],
            /* [size_is][in] */ ULONG32 lines[  ],
            /* [size_is][in] */ ULONG32 columns[  ],
            /* [size_is][in] */ ULONG32 endLines[  ],
            /* [size_is][in] */ ULONG32 endColumns[  ]) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedMethodVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedMethod * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedMethod * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedMethod * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetToken )( 
            ISymUnmanagedMethod * This,
            /* [retval][out] */ mdMethodDef *pToken);
        
        HRESULT ( STDMETHODCALLTYPE *GetSequencePointCount )( 
            ISymUnmanagedMethod * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetRootScope )( 
            ISymUnmanagedMethod * This,
            /* [retval][out] */ ISymUnmanagedScope **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetScopeFromOffset )( 
            ISymUnmanagedMethod * This,
            /* [in] */ ULONG32 offset,
            /* [retval][out] */ ISymUnmanagedScope **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetOffset )( 
            ISymUnmanagedMethod * This,
            /* [in] */ ISymUnmanagedDocument *document,
            /* [in] */ ULONG32 line,
            /* [in] */ ULONG32 column,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetRanges )( 
            ISymUnmanagedMethod * This,
            /* [in] */ ISymUnmanagedDocument *document,
            /* [in] */ ULONG32 line,
            /* [in] */ ULONG32 column,
            /* [in] */ ULONG32 cRanges,
            /* [out] */ ULONG32 *pcRanges,
            /* [length_is][size_is][out] */ ULONG32 ranges[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetParameters )( 
            ISymUnmanagedMethod * This,
            /* [in] */ ULONG32 cParams,
            /* [out] */ ULONG32 *pcParams,
            /* [length_is][size_is][out] */ ISymUnmanagedVariable *params[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetNamespace )( 
            ISymUnmanagedMethod * This,
            /* [out] */ ISymUnmanagedNamespace **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetSourceStartEnd )( 
            ISymUnmanagedMethod * This,
            /* [in] */ ISymUnmanagedDocument *docs[ 2 ],
            /* [in] */ ULONG32 lines[ 2 ],
            /* [in] */ ULONG32 columns[ 2 ],
            /* [out] */ BOOL *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetSequencePoints )( 
            ISymUnmanagedMethod * This,
            /* [in] */ ULONG32 cPoints,
            /* [out] */ ULONG32 *pcPoints,
            /* [size_is][in] */ ULONG32 offsets[  ],
            /* [size_is][in] */ ISymUnmanagedDocument *documents[  ],
            /* [size_is][in] */ ULONG32 lines[  ],
            /* [size_is][in] */ ULONG32 columns[  ],
            /* [size_is][in] */ ULONG32 endLines[  ],
            /* [size_is][in] */ ULONG32 endColumns[  ]);
        
        END_INTERFACE
    } ISymUnmanagedMethodVtbl;

    interface ISymUnmanagedMethod
    {
        CONST_VTBL struct ISymUnmanagedMethodVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedMethod_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedMethod_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedMethod_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedMethod_GetToken(This,pToken)	\
    (This)->lpVtbl -> GetToken(This,pToken)

#define ISymUnmanagedMethod_GetSequencePointCount(This,pRetVal)	\
    (This)->lpVtbl -> GetSequencePointCount(This,pRetVal)

#define ISymUnmanagedMethod_GetRootScope(This,pRetVal)	\
    (This)->lpVtbl -> GetRootScope(This,pRetVal)

#define ISymUnmanagedMethod_GetScopeFromOffset(This,offset,pRetVal)	\
    (This)->lpVtbl -> GetScopeFromOffset(This,offset,pRetVal)

#define ISymUnmanagedMethod_GetOffset(This,document,line,column,pRetVal)	\
    (This)->lpVtbl -> GetOffset(This,document,line,column,pRetVal)

#define ISymUnmanagedMethod_GetRanges(This,document,line,column,cRanges,pcRanges,ranges)	\
    (This)->lpVtbl -> GetRanges(This,document,line,column,cRanges,pcRanges,ranges)

#define ISymUnmanagedMethod_GetParameters(This,cParams,pcParams,params)	\
    (This)->lpVtbl -> GetParameters(This,cParams,pcParams,params)

#define ISymUnmanagedMethod_GetNamespace(This,pRetVal)	\
    (This)->lpVtbl -> GetNamespace(This,pRetVal)

#define ISymUnmanagedMethod_GetSourceStartEnd(This,docs,lines,columns,pRetVal)	\
    (This)->lpVtbl -> GetSourceStartEnd(This,docs,lines,columns,pRetVal)

#define ISymUnmanagedMethod_GetSequencePoints(This,cPoints,pcPoints,offsets,documents,lines,columns,endLines,endColumns)	\
    (This)->lpVtbl -> GetSequencePoints(This,cPoints,pcPoints,offsets,documents,lines,columns,endLines,endColumns)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedMethod_GetToken_Proxy( 
    ISymUnmanagedMethod * This,
    /* [retval][out] */ mdMethodDef *pToken);


void __RPC_STUB ISymUnmanagedMethod_GetToken_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedMethod_GetSequencePointCount_Proxy( 
    ISymUnmanagedMethod * This,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedMethod_GetSequencePointCount_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedMethod_GetRootScope_Proxy( 
    ISymUnmanagedMethod * This,
    /* [retval][out] */ ISymUnmanagedScope **pRetVal);


void __RPC_STUB ISymUnmanagedMethod_GetRootScope_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedMethod_GetScopeFromOffset_Proxy( 
    ISymUnmanagedMethod * This,
    /* [in] */ ULONG32 offset,
    /* [retval][out] */ ISymUnmanagedScope **pRetVal);


void __RPC_STUB ISymUnmanagedMethod_GetScopeFromOffset_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedMethod_GetOffset_Proxy( 
    ISymUnmanagedMethod * This,
    /* [in] */ ISymUnmanagedDocument *document,
    /* [in] */ ULONG32 line,
    /* [in] */ ULONG32 column,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedMethod_GetOffset_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedMethod_GetRanges_Proxy( 
    ISymUnmanagedMethod * This,
    /* [in] */ ISymUnmanagedDocument *document,
    /* [in] */ ULONG32 line,
    /* [in] */ ULONG32 column,
    /* [in] */ ULONG32 cRanges,
    /* [out] */ ULONG32 *pcRanges,
    /* [length_is][size_is][out] */ ULONG32 ranges[  ]);


void __RPC_STUB ISymUnmanagedMethod_GetRanges_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedMethod_GetParameters_Proxy( 
    ISymUnmanagedMethod * This,
    /* [in] */ ULONG32 cParams,
    /* [out] */ ULONG32 *pcParams,
    /* [length_is][size_is][out] */ ISymUnmanagedVariable *params[  ]);


void __RPC_STUB ISymUnmanagedMethod_GetParameters_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedMethod_GetNamespace_Proxy( 
    ISymUnmanagedMethod * This,
    /* [out] */ ISymUnmanagedNamespace **pRetVal);


void __RPC_STUB ISymUnmanagedMethod_GetNamespace_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedMethod_GetSourceStartEnd_Proxy( 
    ISymUnmanagedMethod * This,
    /* [in] */ ISymUnmanagedDocument *docs[ 2 ],
    /* [in] */ ULONG32 lines[ 2 ],
    /* [in] */ ULONG32 columns[ 2 ],
    /* [out] */ BOOL *pRetVal);


void __RPC_STUB ISymUnmanagedMethod_GetSourceStartEnd_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedMethod_GetSequencePoints_Proxy( 
    ISymUnmanagedMethod * This,
    /* [in] */ ULONG32 cPoints,
    /* [out] */ ULONG32 *pcPoints,
    /* [size_is][in] */ ULONG32 offsets[  ],
    /* [size_is][in] */ ISymUnmanagedDocument *documents[  ],
    /* [size_is][in] */ ULONG32 lines[  ],
    /* [size_is][in] */ ULONG32 columns[  ],
    /* [size_is][in] */ ULONG32 endLines[  ],
    /* [size_is][in] */ ULONG32 endColumns[  ]);


void __RPC_STUB ISymUnmanagedMethod_GetSequencePoints_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedMethod_INTERFACE_DEFINED__ */


#ifndef __ISymENCUnmanagedMethod_INTERFACE_DEFINED__
#define __ISymENCUnmanagedMethod_INTERFACE_DEFINED__

/* interface ISymENCUnmanagedMethod */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymENCUnmanagedMethod;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("85E891DA-A631-4c76-ACA2-A44A39C46B8C")
    ISymENCUnmanagedMethod : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetFileNameFromOffset( 
            /* [in] */ ULONG32 dwOffset,
            /* [in] */ ULONG32 cchName,
            /* [out] */ ULONG32 *pcchName,
            /* [length_is][size_is][out] */ WCHAR szName[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetLineFromOffset( 
            /* [in] */ ULONG32 dwOffset,
            /* [out] */ ULONG32 *pline,
            /* [out] */ ULONG32 *pcolumn,
            /* [out] */ ULONG32 *pendLine,
            /* [out] */ ULONG32 *pendColumn,
            /* [out] */ ULONG32 *pdwStartOffset) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymENCUnmanagedMethodVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymENCUnmanagedMethod * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymENCUnmanagedMethod * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymENCUnmanagedMethod * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetFileNameFromOffset )( 
            ISymENCUnmanagedMethod * This,
            /* [in] */ ULONG32 dwOffset,
            /* [in] */ ULONG32 cchName,
            /* [out] */ ULONG32 *pcchName,
            /* [length_is][size_is][out] */ WCHAR szName[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetLineFromOffset )( 
            ISymENCUnmanagedMethod * This,
            /* [in] */ ULONG32 dwOffset,
            /* [out] */ ULONG32 *pline,
            /* [out] */ ULONG32 *pcolumn,
            /* [out] */ ULONG32 *pendLine,
            /* [out] */ ULONG32 *pendColumn,
            /* [out] */ ULONG32 *pdwStartOffset);
        
        END_INTERFACE
    } ISymENCUnmanagedMethodVtbl;

    interface ISymENCUnmanagedMethod
    {
        CONST_VTBL struct ISymENCUnmanagedMethodVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymENCUnmanagedMethod_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymENCUnmanagedMethod_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymENCUnmanagedMethod_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymENCUnmanagedMethod_GetFileNameFromOffset(This,dwOffset,cchName,pcchName,szName)	\
    (This)->lpVtbl -> GetFileNameFromOffset(This,dwOffset,cchName,pcchName,szName)

#define ISymENCUnmanagedMethod_GetLineFromOffset(This,dwOffset,pline,pcolumn,pendLine,pendColumn,pdwStartOffset)	\
    (This)->lpVtbl -> GetLineFromOffset(This,dwOffset,pline,pcolumn,pendLine,pendColumn,pdwStartOffset)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymENCUnmanagedMethod_GetFileNameFromOffset_Proxy( 
    ISymENCUnmanagedMethod * This,
    /* [in] */ ULONG32 dwOffset,
    /* [in] */ ULONG32 cchName,
    /* [out] */ ULONG32 *pcchName,
    /* [length_is][size_is][out] */ WCHAR szName[  ]);


void __RPC_STUB ISymENCUnmanagedMethod_GetFileNameFromOffset_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymENCUnmanagedMethod_GetLineFromOffset_Proxy( 
    ISymENCUnmanagedMethod * This,
    /* [in] */ ULONG32 dwOffset,
    /* [out] */ ULONG32 *pline,
    /* [out] */ ULONG32 *pcolumn,
    /* [out] */ ULONG32 *pendLine,
    /* [out] */ ULONG32 *pendColumn,
    /* [out] */ ULONG32 *pdwStartOffset);


void __RPC_STUB ISymENCUnmanagedMethod_GetLineFromOffset_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymENCUnmanagedMethod_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedNamespace_INTERFACE_DEFINED__
#define __ISymUnmanagedNamespace_INTERFACE_DEFINED__

/* interface ISymUnmanagedNamespace */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedNamespace;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("0DFF7289-54F8-11d3-BD28-0000F80849BD")
    ISymUnmanagedNamespace : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetName( 
            /* [in] */ ULONG32 cchName,
            /* [out] */ ULONG32 *pcchName,
            /* [length_is][size_is][out] */ WCHAR szName[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetNamespaces( 
            /* [in] */ ULONG32 cNameSpaces,
            /* [out] */ ULONG32 *pcNameSpaces,
            /* [length_is][size_is][out] */ ISymUnmanagedNamespace *namespaces[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetVariables( 
            /* [in] */ ULONG32 cVars,
            /* [out] */ ULONG32 *pcVars,
            /* [length_is][size_is][out] */ ISymUnmanagedVariable *pVars[  ]) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedNamespaceVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedNamespace * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedNamespace * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedNamespace * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetName )( 
            ISymUnmanagedNamespace * This,
            /* [in] */ ULONG32 cchName,
            /* [out] */ ULONG32 *pcchName,
            /* [length_is][size_is][out] */ WCHAR szName[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetNamespaces )( 
            ISymUnmanagedNamespace * This,
            /* [in] */ ULONG32 cNameSpaces,
            /* [out] */ ULONG32 *pcNameSpaces,
            /* [length_is][size_is][out] */ ISymUnmanagedNamespace *namespaces[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetVariables )( 
            ISymUnmanagedNamespace * This,
            /* [in] */ ULONG32 cVars,
            /* [out] */ ULONG32 *pcVars,
            /* [length_is][size_is][out] */ ISymUnmanagedVariable *pVars[  ]);
        
        END_INTERFACE
    } ISymUnmanagedNamespaceVtbl;

    interface ISymUnmanagedNamespace
    {
        CONST_VTBL struct ISymUnmanagedNamespaceVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedNamespace_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedNamespace_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedNamespace_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedNamespace_GetName(This,cchName,pcchName,szName)	\
    (This)->lpVtbl -> GetName(This,cchName,pcchName,szName)

#define ISymUnmanagedNamespace_GetNamespaces(This,cNameSpaces,pcNameSpaces,namespaces)	\
    (This)->lpVtbl -> GetNamespaces(This,cNameSpaces,pcNameSpaces,namespaces)

#define ISymUnmanagedNamespace_GetVariables(This,cVars,pcVars,pVars)	\
    (This)->lpVtbl -> GetVariables(This,cVars,pcVars,pVars)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedNamespace_GetName_Proxy( 
    ISymUnmanagedNamespace * This,
    /* [in] */ ULONG32 cchName,
    /* [out] */ ULONG32 *pcchName,
    /* [length_is][size_is][out] */ WCHAR szName[  ]);


void __RPC_STUB ISymUnmanagedNamespace_GetName_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedNamespace_GetNamespaces_Proxy( 
    ISymUnmanagedNamespace * This,
    /* [in] */ ULONG32 cNameSpaces,
    /* [out] */ ULONG32 *pcNameSpaces,
    /* [length_is][size_is][out] */ ISymUnmanagedNamespace *namespaces[  ]);


void __RPC_STUB ISymUnmanagedNamespace_GetNamespaces_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedNamespace_GetVariables_Proxy( 
    ISymUnmanagedNamespace * This,
    /* [in] */ ULONG32 cVars,
    /* [out] */ ULONG32 *pcVars,
    /* [length_is][size_is][out] */ ISymUnmanagedVariable *pVars[  ]);


void __RPC_STUB ISymUnmanagedNamespace_GetVariables_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedNamespace_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedReader_INTERFACE_DEFINED__
#define __ISymUnmanagedReader_INTERFACE_DEFINED__

/* interface ISymUnmanagedReader */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedReader;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("B4CE6286-2A6B-3712-A3B7-1EE1DAD467B5")
    ISymUnmanagedReader : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetDocument( 
            /* [in] */ WCHAR *url,
            /* [in] */ GUID language,
            /* [in] */ GUID languageVendor,
            /* [in] */ GUID documentType,
            /* [retval][out] */ ISymUnmanagedDocument **pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetDocuments( 
            /* [in] */ ULONG32 cDocs,
            /* [out] */ ULONG32 *pcDocs,
            /* [length_is][size_is][out] */ ISymUnmanagedDocument *pDocs[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetUserEntryPoint( 
            /* [retval][out] */ mdMethodDef *pToken) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetMethod( 
            /* [in] */ mdMethodDef token,
            /* [retval][out] */ ISymUnmanagedMethod **pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetMethodByVersion( 
            /* [in] */ mdMethodDef token,
            /* [in] */ int version,
            /* [retval][out] */ ISymUnmanagedMethod **pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetVariables( 
            /* [in] */ mdToken parent,
            /* [in] */ ULONG32 cVars,
            /* [out] */ ULONG32 *pcVars,
            /* [length_is][size_is][out] */ ISymUnmanagedVariable *pVars[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetGlobalVariables( 
            /* [in] */ ULONG32 cVars,
            /* [out] */ ULONG32 *pcVars,
            /* [length_is][size_is][out] */ ISymUnmanagedVariable *pVars[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetMethodFromDocumentPosition( 
            /* [in] */ ISymUnmanagedDocument *document,
            /* [in] */ ULONG32 line,
            /* [in] */ ULONG32 column,
            /* [retval][out] */ ISymUnmanagedMethod **pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetSymAttribute( 
            /* [in] */ mdToken parent,
            /* [in] */ WCHAR *name,
            /* [in] */ ULONG32 cBuffer,
            /* [out] */ ULONG32 *pcBuffer,
            /* [length_is][size_is][out] */ BYTE buffer[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetNamespaces( 
            /* [in] */ ULONG32 cNameSpaces,
            /* [out] */ ULONG32 *pcNameSpaces,
            /* [length_is][size_is][out] */ ISymUnmanagedNamespace *namespaces[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE Initialize( 
            /* [in] */ IUnknown *importer,
            /* [in] */ const WCHAR *filename,
            /* [in] */ const WCHAR *searchPath,
            /* [in] */ IStream *pIStream) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE UpdateSymbolStore( 
            /* [in] */ const WCHAR *filename,
            /* [in] */ IStream *pIStream) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE ReplaceSymbolStore( 
            /* [in] */ const WCHAR *filename,
            /* [in] */ IStream *pIStream) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetSymbolStoreFileName( 
            /* [in] */ ULONG32 cchName,
            /* [out] */ ULONG32 *pcchName,
            /* [length_is][size_is][out] */ WCHAR szName[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetMethodsFromDocumentPosition( 
            /* [in] */ ISymUnmanagedDocument *document,
            /* [in] */ ULONG32 line,
            /* [in] */ ULONG32 column,
            /* [in] */ ULONG32 cMethod,
            /* [out] */ ULONG32 *pcMethod,
            /* [length_is][size_is][out] */ ISymUnmanagedMethod *pRetVal[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetDocumentVersion( 
            /* [in] */ ISymUnmanagedDocument *pDoc,
            /* [out] */ int *version,
            /* [out] */ BOOL *pbCurrent) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetMethodVersion( 
            /* [in] */ ISymUnmanagedMethod *pMethod,
            /* [out] */ int *version) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedReaderVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedReader * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedReader * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedReader * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetDocument )( 
            ISymUnmanagedReader * This,
            /* [in] */ WCHAR *url,
            /* [in] */ GUID language,
            /* [in] */ GUID languageVendor,
            /* [in] */ GUID documentType,
            /* [retval][out] */ ISymUnmanagedDocument **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetDocuments )( 
            ISymUnmanagedReader * This,
            /* [in] */ ULONG32 cDocs,
            /* [out] */ ULONG32 *pcDocs,
            /* [length_is][size_is][out] */ ISymUnmanagedDocument *pDocs[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetUserEntryPoint )( 
            ISymUnmanagedReader * This,
            /* [retval][out] */ mdMethodDef *pToken);
        
        HRESULT ( STDMETHODCALLTYPE *GetMethod )( 
            ISymUnmanagedReader * This,
            /* [in] */ mdMethodDef token,
            /* [retval][out] */ ISymUnmanagedMethod **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetMethodByVersion )( 
            ISymUnmanagedReader * This,
            /* [in] */ mdMethodDef token,
            /* [in] */ int version,
            /* [retval][out] */ ISymUnmanagedMethod **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetVariables )( 
            ISymUnmanagedReader * This,
            /* [in] */ mdToken parent,
            /* [in] */ ULONG32 cVars,
            /* [out] */ ULONG32 *pcVars,
            /* [length_is][size_is][out] */ ISymUnmanagedVariable *pVars[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetGlobalVariables )( 
            ISymUnmanagedReader * This,
            /* [in] */ ULONG32 cVars,
            /* [out] */ ULONG32 *pcVars,
            /* [length_is][size_is][out] */ ISymUnmanagedVariable *pVars[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetMethodFromDocumentPosition )( 
            ISymUnmanagedReader * This,
            /* [in] */ ISymUnmanagedDocument *document,
            /* [in] */ ULONG32 line,
            /* [in] */ ULONG32 column,
            /* [retval][out] */ ISymUnmanagedMethod **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetSymAttribute )( 
            ISymUnmanagedReader * This,
            /* [in] */ mdToken parent,
            /* [in] */ WCHAR *name,
            /* [in] */ ULONG32 cBuffer,
            /* [out] */ ULONG32 *pcBuffer,
            /* [length_is][size_is][out] */ BYTE buffer[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetNamespaces )( 
            ISymUnmanagedReader * This,
            /* [in] */ ULONG32 cNameSpaces,
            /* [out] */ ULONG32 *pcNameSpaces,
            /* [length_is][size_is][out] */ ISymUnmanagedNamespace *namespaces[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *Initialize )( 
            ISymUnmanagedReader * This,
            /* [in] */ IUnknown *importer,
            /* [in] */ const WCHAR *filename,
            /* [in] */ const WCHAR *searchPath,
            /* [in] */ IStream *pIStream);
        
        HRESULT ( STDMETHODCALLTYPE *UpdateSymbolStore )( 
            ISymUnmanagedReader * This,
            /* [in] */ const WCHAR *filename,
            /* [in] */ IStream *pIStream);
        
        HRESULT ( STDMETHODCALLTYPE *ReplaceSymbolStore )( 
            ISymUnmanagedReader * This,
            /* [in] */ const WCHAR *filename,
            /* [in] */ IStream *pIStream);
        
        HRESULT ( STDMETHODCALLTYPE *GetSymbolStoreFileName )( 
            ISymUnmanagedReader * This,
            /* [in] */ ULONG32 cchName,
            /* [out] */ ULONG32 *pcchName,
            /* [length_is][size_is][out] */ WCHAR szName[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetMethodsFromDocumentPosition )( 
            ISymUnmanagedReader * This,
            /* [in] */ ISymUnmanagedDocument *document,
            /* [in] */ ULONG32 line,
            /* [in] */ ULONG32 column,
            /* [in] */ ULONG32 cMethod,
            /* [out] */ ULONG32 *pcMethod,
            /* [length_is][size_is][out] */ ISymUnmanagedMethod *pRetVal[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetDocumentVersion )( 
            ISymUnmanagedReader * This,
            /* [in] */ ISymUnmanagedDocument *pDoc,
            /* [out] */ int *version,
            /* [out] */ BOOL *pbCurrent);
        
        HRESULT ( STDMETHODCALLTYPE *GetMethodVersion )( 
            ISymUnmanagedReader * This,
            /* [in] */ ISymUnmanagedMethod *pMethod,
            /* [out] */ int *version);
        
        END_INTERFACE
    } ISymUnmanagedReaderVtbl;

    interface ISymUnmanagedReader
    {
        CONST_VTBL struct ISymUnmanagedReaderVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedReader_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedReader_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedReader_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedReader_GetDocument(This,url,language,languageVendor,documentType,pRetVal)	\
    (This)->lpVtbl -> GetDocument(This,url,language,languageVendor,documentType,pRetVal)

#define ISymUnmanagedReader_GetDocuments(This,cDocs,pcDocs,pDocs)	\
    (This)->lpVtbl -> GetDocuments(This,cDocs,pcDocs,pDocs)

#define ISymUnmanagedReader_GetUserEntryPoint(This,pToken)	\
    (This)->lpVtbl -> GetUserEntryPoint(This,pToken)

#define ISymUnmanagedReader_GetMethod(This,token,pRetVal)	\
    (This)->lpVtbl -> GetMethod(This,token,pRetVal)

#define ISymUnmanagedReader_GetMethodByVersion(This,token,version,pRetVal)	\
    (This)->lpVtbl -> GetMethodByVersion(This,token,version,pRetVal)

#define ISymUnmanagedReader_GetVariables(This,parent,cVars,pcVars,pVars)	\
    (This)->lpVtbl -> GetVariables(This,parent,cVars,pcVars,pVars)

#define ISymUnmanagedReader_GetGlobalVariables(This,cVars,pcVars,pVars)	\
    (This)->lpVtbl -> GetGlobalVariables(This,cVars,pcVars,pVars)

#define ISymUnmanagedReader_GetMethodFromDocumentPosition(This,document,line,column,pRetVal)	\
    (This)->lpVtbl -> GetMethodFromDocumentPosition(This,document,line,column,pRetVal)

#define ISymUnmanagedReader_GetSymAttribute(This,parent,name,cBuffer,pcBuffer,buffer)	\
    (This)->lpVtbl -> GetSymAttribute(This,parent,name,cBuffer,pcBuffer,buffer)

#define ISymUnmanagedReader_GetNamespaces(This,cNameSpaces,pcNameSpaces,namespaces)	\
    (This)->lpVtbl -> GetNamespaces(This,cNameSpaces,pcNameSpaces,namespaces)

#define ISymUnmanagedReader_Initialize(This,importer,filename,searchPath,pIStream)	\
    (This)->lpVtbl -> Initialize(This,importer,filename,searchPath,pIStream)

#define ISymUnmanagedReader_UpdateSymbolStore(This,filename,pIStream)	\
    (This)->lpVtbl -> UpdateSymbolStore(This,filename,pIStream)

#define ISymUnmanagedReader_ReplaceSymbolStore(This,filename,pIStream)	\
    (This)->lpVtbl -> ReplaceSymbolStore(This,filename,pIStream)

#define ISymUnmanagedReader_GetSymbolStoreFileName(This,cchName,pcchName,szName)	\
    (This)->lpVtbl -> GetSymbolStoreFileName(This,cchName,pcchName,szName)

#define ISymUnmanagedReader_GetMethodsFromDocumentPosition(This,document,line,column,cMethod,pcMethod,pRetVal)	\
    (This)->lpVtbl -> GetMethodsFromDocumentPosition(This,document,line,column,cMethod,pcMethod,pRetVal)

#define ISymUnmanagedReader_GetDocumentVersion(This,pDoc,version,pbCurrent)	\
    (This)->lpVtbl -> GetDocumentVersion(This,pDoc,version,pbCurrent)

#define ISymUnmanagedReader_GetMethodVersion(This,pMethod,version)	\
    (This)->lpVtbl -> GetMethodVersion(This,pMethod,version)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_GetDocument_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ WCHAR *url,
    /* [in] */ GUID language,
    /* [in] */ GUID languageVendor,
    /* [in] */ GUID documentType,
    /* [retval][out] */ ISymUnmanagedDocument **pRetVal);


void __RPC_STUB ISymUnmanagedReader_GetDocument_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_GetDocuments_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ ULONG32 cDocs,
    /* [out] */ ULONG32 *pcDocs,
    /* [length_is][size_is][out] */ ISymUnmanagedDocument *pDocs[  ]);


void __RPC_STUB ISymUnmanagedReader_GetDocuments_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_GetUserEntryPoint_Proxy( 
    ISymUnmanagedReader * This,
    /* [retval][out] */ mdMethodDef *pToken);


void __RPC_STUB ISymUnmanagedReader_GetUserEntryPoint_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_GetMethod_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ mdMethodDef token,
    /* [retval][out] */ ISymUnmanagedMethod **pRetVal);


void __RPC_STUB ISymUnmanagedReader_GetMethod_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_GetMethodByVersion_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ mdMethodDef token,
    /* [in] */ int version,
    /* [retval][out] */ ISymUnmanagedMethod **pRetVal);


void __RPC_STUB ISymUnmanagedReader_GetMethodByVersion_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_GetVariables_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ mdToken parent,
    /* [in] */ ULONG32 cVars,
    /* [out] */ ULONG32 *pcVars,
    /* [length_is][size_is][out] */ ISymUnmanagedVariable *pVars[  ]);


void __RPC_STUB ISymUnmanagedReader_GetVariables_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_GetGlobalVariables_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ ULONG32 cVars,
    /* [out] */ ULONG32 *pcVars,
    /* [length_is][size_is][out] */ ISymUnmanagedVariable *pVars[  ]);


void __RPC_STUB ISymUnmanagedReader_GetGlobalVariables_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_GetMethodFromDocumentPosition_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ ISymUnmanagedDocument *document,
    /* [in] */ ULONG32 line,
    /* [in] */ ULONG32 column,
    /* [retval][out] */ ISymUnmanagedMethod **pRetVal);


void __RPC_STUB ISymUnmanagedReader_GetMethodFromDocumentPosition_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_GetSymAttribute_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ mdToken parent,
    /* [in] */ WCHAR *name,
    /* [in] */ ULONG32 cBuffer,
    /* [out] */ ULONG32 *pcBuffer,
    /* [length_is][size_is][out] */ BYTE buffer[  ]);


void __RPC_STUB ISymUnmanagedReader_GetSymAttribute_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_GetNamespaces_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ ULONG32 cNameSpaces,
    /* [out] */ ULONG32 *pcNameSpaces,
    /* [length_is][size_is][out] */ ISymUnmanagedNamespace *namespaces[  ]);


void __RPC_STUB ISymUnmanagedReader_GetNamespaces_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_Initialize_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ IUnknown *importer,
    /* [in] */ const WCHAR *filename,
    /* [in] */ const WCHAR *searchPath,
    /* [in] */ IStream *pIStream);


void __RPC_STUB ISymUnmanagedReader_Initialize_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_UpdateSymbolStore_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ const WCHAR *filename,
    /* [in] */ IStream *pIStream);


void __RPC_STUB ISymUnmanagedReader_UpdateSymbolStore_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_ReplaceSymbolStore_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ const WCHAR *filename,
    /* [in] */ IStream *pIStream);


void __RPC_STUB ISymUnmanagedReader_ReplaceSymbolStore_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_GetSymbolStoreFileName_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ ULONG32 cchName,
    /* [out] */ ULONG32 *pcchName,
    /* [length_is][size_is][out] */ WCHAR szName[  ]);


void __RPC_STUB ISymUnmanagedReader_GetSymbolStoreFileName_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_GetMethodsFromDocumentPosition_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ ISymUnmanagedDocument *document,
    /* [in] */ ULONG32 line,
    /* [in] */ ULONG32 column,
    /* [in] */ ULONG32 cMethod,
    /* [out] */ ULONG32 *pcMethod,
    /* [length_is][size_is][out] */ ISymUnmanagedMethod *pRetVal[  ]);


void __RPC_STUB ISymUnmanagedReader_GetMethodsFromDocumentPosition_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_GetDocumentVersion_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ ISymUnmanagedDocument *pDoc,
    /* [out] */ int *version,
    /* [out] */ BOOL *pbCurrent);


void __RPC_STUB ISymUnmanagedReader_GetDocumentVersion_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader_GetMethodVersion_Proxy( 
    ISymUnmanagedReader * This,
    /* [in] */ ISymUnmanagedMethod *pMethod,
    /* [out] */ int *version);


void __RPC_STUB ISymUnmanagedReader_GetMethodVersion_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedReader_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedENCUpdate_INTERFACE_DEFINED__
#define __ISymUnmanagedENCUpdate_INTERFACE_DEFINED__

/* interface ISymUnmanagedENCUpdate */
/* [unique][uuid][object] */ 

typedef struct _SYMLINEDELTA
    {
    mdMethodDef mdMethod;
    INT32 delta;
    } 	SYMLINEDELTA;


EXTERN_C const IID IID_ISymUnmanagedENCUpdate;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("E502D2DD-8671-4338-8F2A-FC08229628C4")
    ISymUnmanagedENCUpdate : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE UpdateSymbolStore2( 
            /* [in] */ IStream *pIStream,
            /* [in] */ SYMLINEDELTA *pDeltaLines,
            /* [in] */ ULONG cDeltaLines) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetLocalVariableCount( 
            /* [in] */ mdMethodDef mdMethodToken,
            /* [out] */ ULONG *pcLocals) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetLocalVariables( 
            /* [in] */ mdMethodDef mdMethodToken,
            /* [in] */ ULONG cLocals,
            /* [length_is][size_is][out] */ ISymUnmanagedVariable *rgLocals[  ],
            /* [out] */ ULONG *pceltFetched) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE InitializeForEnc( void) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedENCUpdateVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedENCUpdate * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedENCUpdate * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedENCUpdate * This);
        
        HRESULT ( STDMETHODCALLTYPE *UpdateSymbolStore2 )( 
            ISymUnmanagedENCUpdate * This,
            /* [in] */ IStream *pIStream,
            /* [in] */ SYMLINEDELTA *pDeltaLines,
            /* [in] */ ULONG cDeltaLines);
        
        HRESULT ( STDMETHODCALLTYPE *GetLocalVariableCount )( 
            ISymUnmanagedENCUpdate * This,
            /* [in] */ mdMethodDef mdMethodToken,
            /* [out] */ ULONG *pcLocals);
        
        HRESULT ( STDMETHODCALLTYPE *GetLocalVariables )( 
            ISymUnmanagedENCUpdate * This,
            /* [in] */ mdMethodDef mdMethodToken,
            /* [in] */ ULONG cLocals,
            /* [length_is][size_is][out] */ ISymUnmanagedVariable *rgLocals[  ],
            /* [out] */ ULONG *pceltFetched);
        
        HRESULT ( STDMETHODCALLTYPE *InitializeForEnc )( 
            ISymUnmanagedENCUpdate * This);
        
        END_INTERFACE
    } ISymUnmanagedENCUpdateVtbl;

    interface ISymUnmanagedENCUpdate
    {
        CONST_VTBL struct ISymUnmanagedENCUpdateVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedENCUpdate_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedENCUpdate_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedENCUpdate_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedENCUpdate_UpdateSymbolStore2(This,pIStream,pDeltaLines,cDeltaLines)	\
    (This)->lpVtbl -> UpdateSymbolStore2(This,pIStream,pDeltaLines,cDeltaLines)

#define ISymUnmanagedENCUpdate_GetLocalVariableCount(This,mdMethodToken,pcLocals)	\
    (This)->lpVtbl -> GetLocalVariableCount(This,mdMethodToken,pcLocals)

#define ISymUnmanagedENCUpdate_GetLocalVariables(This,mdMethodToken,cLocals,rgLocals,pceltFetched)	\
    (This)->lpVtbl -> GetLocalVariables(This,mdMethodToken,cLocals,rgLocals,pceltFetched)

#define ISymUnmanagedENCUpdate_InitializeForEnc(This)	\
    (This)->lpVtbl -> InitializeForEnc(This)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedENCUpdate_UpdateSymbolStore2_Proxy( 
    ISymUnmanagedENCUpdate * This,
    /* [in] */ IStream *pIStream,
    /* [in] */ SYMLINEDELTA *pDeltaLines,
    /* [in] */ ULONG cDeltaLines);


void __RPC_STUB ISymUnmanagedENCUpdate_UpdateSymbolStore2_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedENCUpdate_GetLocalVariableCount_Proxy( 
    ISymUnmanagedENCUpdate * This,
    /* [in] */ mdMethodDef mdMethodToken,
    /* [out] */ ULONG *pcLocals);


void __RPC_STUB ISymUnmanagedENCUpdate_GetLocalVariableCount_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedENCUpdate_GetLocalVariables_Proxy( 
    ISymUnmanagedENCUpdate * This,
    /* [in] */ mdMethodDef mdMethodToken,
    /* [in] */ ULONG cLocals,
    /* [length_is][size_is][out] */ ISymUnmanagedVariable *rgLocals[  ],
    /* [out] */ ULONG *pceltFetched);


void __RPC_STUB ISymUnmanagedENCUpdate_GetLocalVariables_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedENCUpdate_InitializeForEnc_Proxy( 
    ISymUnmanagedENCUpdate * This);


void __RPC_STUB ISymUnmanagedENCUpdate_InitializeForEnc_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedENCUpdate_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedReaderSymbolSearchInfo_INTERFACE_DEFINED__
#define __ISymUnmanagedReaderSymbolSearchInfo_INTERFACE_DEFINED__

/* interface ISymUnmanagedReaderSymbolSearchInfo */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedReaderSymbolSearchInfo;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("20D9645D-03CD-4e34-9C11-9848A5B084F1")
    ISymUnmanagedReaderSymbolSearchInfo : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetSymbolSearchInfoCount( 
            /* [out] */ ULONG32 *pcSearchInfo) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetSymbolSearchInfo( 
            /* [in] */ ULONG32 cSearchInfo,
            /* [out] */ ULONG32 *pcSearchInfo,
            /* [length_is][size_is][out] */ ISymUnmanagedSymbolSearchInfo **rgpSearchInfo) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedReaderSymbolSearchInfoVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedReaderSymbolSearchInfo * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedReaderSymbolSearchInfo * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedReaderSymbolSearchInfo * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetSymbolSearchInfoCount )( 
            ISymUnmanagedReaderSymbolSearchInfo * This,
            /* [out] */ ULONG32 *pcSearchInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetSymbolSearchInfo )( 
            ISymUnmanagedReaderSymbolSearchInfo * This,
            /* [in] */ ULONG32 cSearchInfo,
            /* [out] */ ULONG32 *pcSearchInfo,
            /* [length_is][size_is][out] */ ISymUnmanagedSymbolSearchInfo **rgpSearchInfo);
        
        END_INTERFACE
    } ISymUnmanagedReaderSymbolSearchInfoVtbl;

    interface ISymUnmanagedReaderSymbolSearchInfo
    {
        CONST_VTBL struct ISymUnmanagedReaderSymbolSearchInfoVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedReaderSymbolSearchInfo_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedReaderSymbolSearchInfo_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedReaderSymbolSearchInfo_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedReaderSymbolSearchInfo_GetSymbolSearchInfoCount(This,pcSearchInfo)	\
    (This)->lpVtbl -> GetSymbolSearchInfoCount(This,pcSearchInfo)

#define ISymUnmanagedReaderSymbolSearchInfo_GetSymbolSearchInfo(This,cSearchInfo,pcSearchInfo,rgpSearchInfo)	\
    (This)->lpVtbl -> GetSymbolSearchInfo(This,cSearchInfo,pcSearchInfo,rgpSearchInfo)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedReaderSymbolSearchInfo_GetSymbolSearchInfoCount_Proxy( 
    ISymUnmanagedReaderSymbolSearchInfo * This,
    /* [out] */ ULONG32 *pcSearchInfo);


void __RPC_STUB ISymUnmanagedReaderSymbolSearchInfo_GetSymbolSearchInfoCount_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReaderSymbolSearchInfo_GetSymbolSearchInfo_Proxy( 
    ISymUnmanagedReaderSymbolSearchInfo * This,
    /* [in] */ ULONG32 cSearchInfo,
    /* [out] */ ULONG32 *pcSearchInfo,
    /* [length_is][size_is][out] */ ISymUnmanagedSymbolSearchInfo **rgpSearchInfo);


void __RPC_STUB ISymUnmanagedReaderSymbolSearchInfo_GetSymbolSearchInfo_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedReaderSymbolSearchInfo_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedScope_INTERFACE_DEFINED__
#define __ISymUnmanagedScope_INTERFACE_DEFINED__

/* interface ISymUnmanagedScope */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedScope;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("68005D0F-B8E0-3B01-84D5-A11A94154942")
    ISymUnmanagedScope : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetMethod( 
            /* [retval][out] */ ISymUnmanagedMethod **pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetParent( 
            /* [retval][out] */ ISymUnmanagedScope **pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetChildren( 
            /* [in] */ ULONG32 cChildren,
            /* [out] */ ULONG32 *pcChildren,
            /* [length_is][size_is][out] */ ISymUnmanagedScope *children[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetStartOffset( 
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetEndOffset( 
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetLocalCount( 
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetLocals( 
            /* [in] */ ULONG32 cLocals,
            /* [out] */ ULONG32 *pcLocals,
            /* [length_is][size_is][out] */ ISymUnmanagedVariable *locals[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetNamespaces( 
            /* [in] */ ULONG32 cNameSpaces,
            /* [out] */ ULONG32 *pcNameSpaces,
            /* [length_is][size_is][out] */ ISymUnmanagedNamespace *namespaces[  ]) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedScopeVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedScope * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedScope * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedScope * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetMethod )( 
            ISymUnmanagedScope * This,
            /* [retval][out] */ ISymUnmanagedMethod **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetParent )( 
            ISymUnmanagedScope * This,
            /* [retval][out] */ ISymUnmanagedScope **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetChildren )( 
            ISymUnmanagedScope * This,
            /* [in] */ ULONG32 cChildren,
            /* [out] */ ULONG32 *pcChildren,
            /* [length_is][size_is][out] */ ISymUnmanagedScope *children[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetStartOffset )( 
            ISymUnmanagedScope * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetEndOffset )( 
            ISymUnmanagedScope * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetLocalCount )( 
            ISymUnmanagedScope * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetLocals )( 
            ISymUnmanagedScope * This,
            /* [in] */ ULONG32 cLocals,
            /* [out] */ ULONG32 *pcLocals,
            /* [length_is][size_is][out] */ ISymUnmanagedVariable *locals[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetNamespaces )( 
            ISymUnmanagedScope * This,
            /* [in] */ ULONG32 cNameSpaces,
            /* [out] */ ULONG32 *pcNameSpaces,
            /* [length_is][size_is][out] */ ISymUnmanagedNamespace *namespaces[  ]);
        
        END_INTERFACE
    } ISymUnmanagedScopeVtbl;

    interface ISymUnmanagedScope
    {
        CONST_VTBL struct ISymUnmanagedScopeVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedScope_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedScope_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedScope_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedScope_GetMethod(This,pRetVal)	\
    (This)->lpVtbl -> GetMethod(This,pRetVal)

#define ISymUnmanagedScope_GetParent(This,pRetVal)	\
    (This)->lpVtbl -> GetParent(This,pRetVal)

#define ISymUnmanagedScope_GetChildren(This,cChildren,pcChildren,children)	\
    (This)->lpVtbl -> GetChildren(This,cChildren,pcChildren,children)

#define ISymUnmanagedScope_GetStartOffset(This,pRetVal)	\
    (This)->lpVtbl -> GetStartOffset(This,pRetVal)

#define ISymUnmanagedScope_GetEndOffset(This,pRetVal)	\
    (This)->lpVtbl -> GetEndOffset(This,pRetVal)

#define ISymUnmanagedScope_GetLocalCount(This,pRetVal)	\
    (This)->lpVtbl -> GetLocalCount(This,pRetVal)

#define ISymUnmanagedScope_GetLocals(This,cLocals,pcLocals,locals)	\
    (This)->lpVtbl -> GetLocals(This,cLocals,pcLocals,locals)

#define ISymUnmanagedScope_GetNamespaces(This,cNameSpaces,pcNameSpaces,namespaces)	\
    (This)->lpVtbl -> GetNamespaces(This,cNameSpaces,pcNameSpaces,namespaces)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedScope_GetMethod_Proxy( 
    ISymUnmanagedScope * This,
    /* [retval][out] */ ISymUnmanagedMethod **pRetVal);


void __RPC_STUB ISymUnmanagedScope_GetMethod_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedScope_GetParent_Proxy( 
    ISymUnmanagedScope * This,
    /* [retval][out] */ ISymUnmanagedScope **pRetVal);


void __RPC_STUB ISymUnmanagedScope_GetParent_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedScope_GetChildren_Proxy( 
    ISymUnmanagedScope * This,
    /* [in] */ ULONG32 cChildren,
    /* [out] */ ULONG32 *pcChildren,
    /* [length_is][size_is][out] */ ISymUnmanagedScope *children[  ]);


void __RPC_STUB ISymUnmanagedScope_GetChildren_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedScope_GetStartOffset_Proxy( 
    ISymUnmanagedScope * This,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedScope_GetStartOffset_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedScope_GetEndOffset_Proxy( 
    ISymUnmanagedScope * This,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedScope_GetEndOffset_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedScope_GetLocalCount_Proxy( 
    ISymUnmanagedScope * This,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedScope_GetLocalCount_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedScope_GetLocals_Proxy( 
    ISymUnmanagedScope * This,
    /* [in] */ ULONG32 cLocals,
    /* [out] */ ULONG32 *pcLocals,
    /* [length_is][size_is][out] */ ISymUnmanagedVariable *locals[  ]);


void __RPC_STUB ISymUnmanagedScope_GetLocals_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedScope_GetNamespaces_Proxy( 
    ISymUnmanagedScope * This,
    /* [in] */ ULONG32 cNameSpaces,
    /* [out] */ ULONG32 *pcNameSpaces,
    /* [length_is][size_is][out] */ ISymUnmanagedNamespace *namespaces[  ]);


void __RPC_STUB ISymUnmanagedScope_GetNamespaces_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedScope_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedConstant_INTERFACE_DEFINED__
#define __ISymUnmanagedConstant_INTERFACE_DEFINED__

/* interface ISymUnmanagedConstant */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedConstant;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("48B25ED8-5BAD-41bc-9CEE-CD62FABC74E9")
    ISymUnmanagedConstant : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetName( 
            /* [in] */ ULONG32 cchName,
            /* [out] */ ULONG32 *pcchName,
            /* [length_is][size_is][out] */ WCHAR szName[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetValue( 
            VARIANT *pValue) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetSignature( 
            /* [in] */ ULONG32 cSig,
            /* [out] */ ULONG32 *pcSig,
            /* [length_is][size_is][out] */ BYTE sig[  ]) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedConstantVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedConstant * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedConstant * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedConstant * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetName )( 
            ISymUnmanagedConstant * This,
            /* [in] */ ULONG32 cchName,
            /* [out] */ ULONG32 *pcchName,
            /* [length_is][size_is][out] */ WCHAR szName[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetValue )( 
            ISymUnmanagedConstant * This,
            VARIANT *pValue);
        
        HRESULT ( STDMETHODCALLTYPE *GetSignature )( 
            ISymUnmanagedConstant * This,
            /* [in] */ ULONG32 cSig,
            /* [out] */ ULONG32 *pcSig,
            /* [length_is][size_is][out] */ BYTE sig[  ]);
        
        END_INTERFACE
    } ISymUnmanagedConstantVtbl;

    interface ISymUnmanagedConstant
    {
        CONST_VTBL struct ISymUnmanagedConstantVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedConstant_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedConstant_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedConstant_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedConstant_GetName(This,cchName,pcchName,szName)	\
    (This)->lpVtbl -> GetName(This,cchName,pcchName,szName)

#define ISymUnmanagedConstant_GetValue(This,pValue)	\
    (This)->lpVtbl -> GetValue(This,pValue)

#define ISymUnmanagedConstant_GetSignature(This,cSig,pcSig,sig)	\
    (This)->lpVtbl -> GetSignature(This,cSig,pcSig,sig)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedConstant_GetName_Proxy( 
    ISymUnmanagedConstant * This,
    /* [in] */ ULONG32 cchName,
    /* [out] */ ULONG32 *pcchName,
    /* [length_is][size_is][out] */ WCHAR szName[  ]);


void __RPC_STUB ISymUnmanagedConstant_GetName_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedConstant_GetValue_Proxy( 
    ISymUnmanagedConstant * This,
    VARIANT *pValue);


void __RPC_STUB ISymUnmanagedConstant_GetValue_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedConstant_GetSignature_Proxy( 
    ISymUnmanagedConstant * This,
    /* [in] */ ULONG32 cSig,
    /* [out] */ ULONG32 *pcSig,
    /* [length_is][size_is][out] */ BYTE sig[  ]);


void __RPC_STUB ISymUnmanagedConstant_GetSignature_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedConstant_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedScope2_INTERFACE_DEFINED__
#define __ISymUnmanagedScope2_INTERFACE_DEFINED__

/* interface ISymUnmanagedScope2 */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedScope2;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("AE932FBA-3FD8-4dba-8232-30A2309B02DB")
    ISymUnmanagedScope2 : public ISymUnmanagedScope
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetConstantCount( 
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetConstants( 
            /* [in] */ ULONG32 cConstants,
            /* [out] */ ULONG32 *pcConstants,
            /* [length_is][size_is][out] */ ISymUnmanagedConstant *constants[  ]) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedScope2Vtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedScope2 * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedScope2 * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedScope2 * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetMethod )( 
            ISymUnmanagedScope2 * This,
            /* [retval][out] */ ISymUnmanagedMethod **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetParent )( 
            ISymUnmanagedScope2 * This,
            /* [retval][out] */ ISymUnmanagedScope **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetChildren )( 
            ISymUnmanagedScope2 * This,
            /* [in] */ ULONG32 cChildren,
            /* [out] */ ULONG32 *pcChildren,
            /* [length_is][size_is][out] */ ISymUnmanagedScope *children[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetStartOffset )( 
            ISymUnmanagedScope2 * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetEndOffset )( 
            ISymUnmanagedScope2 * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetLocalCount )( 
            ISymUnmanagedScope2 * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetLocals )( 
            ISymUnmanagedScope2 * This,
            /* [in] */ ULONG32 cLocals,
            /* [out] */ ULONG32 *pcLocals,
            /* [length_is][size_is][out] */ ISymUnmanagedVariable *locals[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetNamespaces )( 
            ISymUnmanagedScope2 * This,
            /* [in] */ ULONG32 cNameSpaces,
            /* [out] */ ULONG32 *pcNameSpaces,
            /* [length_is][size_is][out] */ ISymUnmanagedNamespace *namespaces[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetConstantCount )( 
            ISymUnmanagedScope2 * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetConstants )( 
            ISymUnmanagedScope2 * This,
            /* [in] */ ULONG32 cConstants,
            /* [out] */ ULONG32 *pcConstants,
            /* [length_is][size_is][out] */ ISymUnmanagedConstant *constants[  ]);
        
        END_INTERFACE
    } ISymUnmanagedScope2Vtbl;

    interface ISymUnmanagedScope2
    {
        CONST_VTBL struct ISymUnmanagedScope2Vtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedScope2_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedScope2_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedScope2_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedScope2_GetMethod(This,pRetVal)	\
    (This)->lpVtbl -> GetMethod(This,pRetVal)

#define ISymUnmanagedScope2_GetParent(This,pRetVal)	\
    (This)->lpVtbl -> GetParent(This,pRetVal)

#define ISymUnmanagedScope2_GetChildren(This,cChildren,pcChildren,children)	\
    (This)->lpVtbl -> GetChildren(This,cChildren,pcChildren,children)

#define ISymUnmanagedScope2_GetStartOffset(This,pRetVal)	\
    (This)->lpVtbl -> GetStartOffset(This,pRetVal)

#define ISymUnmanagedScope2_GetEndOffset(This,pRetVal)	\
    (This)->lpVtbl -> GetEndOffset(This,pRetVal)

#define ISymUnmanagedScope2_GetLocalCount(This,pRetVal)	\
    (This)->lpVtbl -> GetLocalCount(This,pRetVal)

#define ISymUnmanagedScope2_GetLocals(This,cLocals,pcLocals,locals)	\
    (This)->lpVtbl -> GetLocals(This,cLocals,pcLocals,locals)

#define ISymUnmanagedScope2_GetNamespaces(This,cNameSpaces,pcNameSpaces,namespaces)	\
    (This)->lpVtbl -> GetNamespaces(This,cNameSpaces,pcNameSpaces,namespaces)


#define ISymUnmanagedScope2_GetConstantCount(This,pRetVal)	\
    (This)->lpVtbl -> GetConstantCount(This,pRetVal)

#define ISymUnmanagedScope2_GetConstants(This,cConstants,pcConstants,constants)	\
    (This)->lpVtbl -> GetConstants(This,cConstants,pcConstants,constants)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedScope2_GetConstantCount_Proxy( 
    ISymUnmanagedScope2 * This,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedScope2_GetConstantCount_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedScope2_GetConstants_Proxy( 
    ISymUnmanagedScope2 * This,
    /* [in] */ ULONG32 cConstants,
    /* [out] */ ULONG32 *pcConstants,
    /* [length_is][size_is][out] */ ISymUnmanagedConstant *constants[  ]);


void __RPC_STUB ISymUnmanagedScope2_GetConstants_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedScope2_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedVariable_INTERFACE_DEFINED__
#define __ISymUnmanagedVariable_INTERFACE_DEFINED__

/* interface ISymUnmanagedVariable */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedVariable;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("9F60EEBE-2D9A-3F7C-BF58-80BC991C60BB")
    ISymUnmanagedVariable : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetName( 
            /* [in] */ ULONG32 cchName,
            /* [out] */ ULONG32 *pcchName,
            /* [length_is][size_is][out] */ WCHAR szName[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetAttributes( 
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetSignature( 
            /* [in] */ ULONG32 cSig,
            /* [out] */ ULONG32 *pcSig,
            /* [length_is][size_is][out] */ BYTE sig[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetAddressKind( 
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetAddressField1( 
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetAddressField2( 
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetAddressField3( 
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetStartOffset( 
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetEndOffset( 
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedVariableVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedVariable * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedVariable * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedVariable * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetName )( 
            ISymUnmanagedVariable * This,
            /* [in] */ ULONG32 cchName,
            /* [out] */ ULONG32 *pcchName,
            /* [length_is][size_is][out] */ WCHAR szName[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetAttributes )( 
            ISymUnmanagedVariable * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetSignature )( 
            ISymUnmanagedVariable * This,
            /* [in] */ ULONG32 cSig,
            /* [out] */ ULONG32 *pcSig,
            /* [length_is][size_is][out] */ BYTE sig[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetAddressKind )( 
            ISymUnmanagedVariable * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetAddressField1 )( 
            ISymUnmanagedVariable * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetAddressField2 )( 
            ISymUnmanagedVariable * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetAddressField3 )( 
            ISymUnmanagedVariable * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetStartOffset )( 
            ISymUnmanagedVariable * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetEndOffset )( 
            ISymUnmanagedVariable * This,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        END_INTERFACE
    } ISymUnmanagedVariableVtbl;

    interface ISymUnmanagedVariable
    {
        CONST_VTBL struct ISymUnmanagedVariableVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedVariable_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedVariable_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedVariable_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedVariable_GetName(This,cchName,pcchName,szName)	\
    (This)->lpVtbl -> GetName(This,cchName,pcchName,szName)

#define ISymUnmanagedVariable_GetAttributes(This,pRetVal)	\
    (This)->lpVtbl -> GetAttributes(This,pRetVal)

#define ISymUnmanagedVariable_GetSignature(This,cSig,pcSig,sig)	\
    (This)->lpVtbl -> GetSignature(This,cSig,pcSig,sig)

#define ISymUnmanagedVariable_GetAddressKind(This,pRetVal)	\
    (This)->lpVtbl -> GetAddressKind(This,pRetVal)

#define ISymUnmanagedVariable_GetAddressField1(This,pRetVal)	\
    (This)->lpVtbl -> GetAddressField1(This,pRetVal)

#define ISymUnmanagedVariable_GetAddressField2(This,pRetVal)	\
    (This)->lpVtbl -> GetAddressField2(This,pRetVal)

#define ISymUnmanagedVariable_GetAddressField3(This,pRetVal)	\
    (This)->lpVtbl -> GetAddressField3(This,pRetVal)

#define ISymUnmanagedVariable_GetStartOffset(This,pRetVal)	\
    (This)->lpVtbl -> GetStartOffset(This,pRetVal)

#define ISymUnmanagedVariable_GetEndOffset(This,pRetVal)	\
    (This)->lpVtbl -> GetEndOffset(This,pRetVal)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedVariable_GetName_Proxy( 
    ISymUnmanagedVariable * This,
    /* [in] */ ULONG32 cchName,
    /* [out] */ ULONG32 *pcchName,
    /* [length_is][size_is][out] */ WCHAR szName[  ]);


void __RPC_STUB ISymUnmanagedVariable_GetName_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedVariable_GetAttributes_Proxy( 
    ISymUnmanagedVariable * This,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedVariable_GetAttributes_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedVariable_GetSignature_Proxy( 
    ISymUnmanagedVariable * This,
    /* [in] */ ULONG32 cSig,
    /* [out] */ ULONG32 *pcSig,
    /* [length_is][size_is][out] */ BYTE sig[  ]);


void __RPC_STUB ISymUnmanagedVariable_GetSignature_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedVariable_GetAddressKind_Proxy( 
    ISymUnmanagedVariable * This,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedVariable_GetAddressKind_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedVariable_GetAddressField1_Proxy( 
    ISymUnmanagedVariable * This,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedVariable_GetAddressField1_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedVariable_GetAddressField2_Proxy( 
    ISymUnmanagedVariable * This,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedVariable_GetAddressField2_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedVariable_GetAddressField3_Proxy( 
    ISymUnmanagedVariable * This,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedVariable_GetAddressField3_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedVariable_GetStartOffset_Proxy( 
    ISymUnmanagedVariable * This,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedVariable_GetStartOffset_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedVariable_GetEndOffset_Proxy( 
    ISymUnmanagedVariable * This,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedVariable_GetEndOffset_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedVariable_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedSymbolSearchInfo_INTERFACE_DEFINED__
#define __ISymUnmanagedSymbolSearchInfo_INTERFACE_DEFINED__

/* interface ISymUnmanagedSymbolSearchInfo */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedSymbolSearchInfo;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("F8B3534A-A46B-4980-B520-BEC4ACEABA8F")
    ISymUnmanagedSymbolSearchInfo : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetSearchPathLength( 
            /* [out] */ ULONG32 *pcchPath) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetSearchPath( 
            /* [in] */ ULONG32 cchPath,
            /* [out] */ ULONG32 *pcchPath,
            /* [length_is][size_is][out] */ WCHAR szPath[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetHRESULT( 
            /* [out] */ HRESULT *phr) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedSymbolSearchInfoVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedSymbolSearchInfo * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedSymbolSearchInfo * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedSymbolSearchInfo * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetSearchPathLength )( 
            ISymUnmanagedSymbolSearchInfo * This,
            /* [out] */ ULONG32 *pcchPath);
        
        HRESULT ( STDMETHODCALLTYPE *GetSearchPath )( 
            ISymUnmanagedSymbolSearchInfo * This,
            /* [in] */ ULONG32 cchPath,
            /* [out] */ ULONG32 *pcchPath,
            /* [length_is][size_is][out] */ WCHAR szPath[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetHRESULT )( 
            ISymUnmanagedSymbolSearchInfo * This,
            /* [out] */ HRESULT *phr);
        
        END_INTERFACE
    } ISymUnmanagedSymbolSearchInfoVtbl;

    interface ISymUnmanagedSymbolSearchInfo
    {
        CONST_VTBL struct ISymUnmanagedSymbolSearchInfoVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedSymbolSearchInfo_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedSymbolSearchInfo_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedSymbolSearchInfo_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedSymbolSearchInfo_GetSearchPathLength(This,pcchPath)	\
    (This)->lpVtbl -> GetSearchPathLength(This,pcchPath)

#define ISymUnmanagedSymbolSearchInfo_GetSearchPath(This,cchPath,pcchPath,szPath)	\
    (This)->lpVtbl -> GetSearchPath(This,cchPath,pcchPath,szPath)

#define ISymUnmanagedSymbolSearchInfo_GetHRESULT(This,phr)	\
    (This)->lpVtbl -> GetHRESULT(This,phr)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedSymbolSearchInfo_GetSearchPathLength_Proxy( 
    ISymUnmanagedSymbolSearchInfo * This,
    /* [out] */ ULONG32 *pcchPath);


void __RPC_STUB ISymUnmanagedSymbolSearchInfo_GetSearchPathLength_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedSymbolSearchInfo_GetSearchPath_Proxy( 
    ISymUnmanagedSymbolSearchInfo * This,
    /* [in] */ ULONG32 cchPath,
    /* [out] */ ULONG32 *pcchPath,
    /* [length_is][size_is][out] */ WCHAR szPath[  ]);


void __RPC_STUB ISymUnmanagedSymbolSearchInfo_GetSearchPath_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedSymbolSearchInfo_GetHRESULT_Proxy( 
    ISymUnmanagedSymbolSearchInfo * This,
    /* [out] */ HRESULT *phr);


void __RPC_STUB ISymUnmanagedSymbolSearchInfo_GetHRESULT_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedSymbolSearchInfo_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedWriter_INTERFACE_DEFINED__
#define __ISymUnmanagedWriter_INTERFACE_DEFINED__

/* interface ISymUnmanagedWriter */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedWriter;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("ED14AA72-78E2-4884-84E2-334293AE5214")
    ISymUnmanagedWriter : public IUnknown
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE DefineDocument( 
            /* [in] */ const WCHAR *url,
            /* [in] */ const GUID *language,
            /* [in] */ const GUID *languageVendor,
            /* [in] */ const GUID *documentType,
            /* [retval][out] */ ISymUnmanagedDocumentWriter **pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE SetUserEntryPoint( 
            /* [in] */ mdMethodDef entryMethod) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE OpenMethod( 
            /* [in] */ mdMethodDef method) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE CloseMethod( void) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE OpenScope( 
            /* [in] */ ULONG32 startOffset,
            /* [retval][out] */ ULONG32 *pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE CloseScope( 
            /* [in] */ ULONG32 endOffset) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE SetScopeRange( 
            /* [in] */ ULONG32 scopeID,
            /* [in] */ ULONG32 startOffset,
            /* [in] */ ULONG32 endOffset) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE DefineLocalVariable( 
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ],
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3,
            /* [in] */ ULONG32 startOffset,
            /* [in] */ ULONG32 endOffset) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE DefineParameter( 
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 sequence,
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE DefineField( 
            /* [in] */ mdTypeDef parent,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ],
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE DefineGlobalVariable( 
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ],
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE Close( void) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE SetSymAttribute( 
            /* [in] */ mdToken parent,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 cData,
            /* [size_is][in] */ unsigned char data[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE OpenNamespace( 
            /* [in] */ const WCHAR *name) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE CloseNamespace( void) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE UsingNamespace( 
            /* [in] */ const WCHAR *fullName) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE SetMethodSourceRange( 
            /* [in] */ ISymUnmanagedDocumentWriter *startDoc,
            /* [in] */ ULONG32 startLine,
            /* [in] */ ULONG32 startColumn,
            /* [in] */ ISymUnmanagedDocumentWriter *endDoc,
            /* [in] */ ULONG32 endLine,
            /* [in] */ ULONG32 endColumn) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE Initialize( 
            /* [in] */ IUnknown *emitter,
            /* [in] */ const WCHAR *filename,
            /* [in] */ IStream *pIStream,
            /* [in] */ BOOL fFullBuild) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetDebugInfo( 
            /* [out][in] */ IMAGE_DEBUG_DIRECTORY *pIDD,
            /* [in] */ DWORD cData,
            /* [out] */ DWORD *pcData,
            /* [length_is][size_is][out] */ BYTE data[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE DefineSequencePoints( 
            /* [in] */ ISymUnmanagedDocumentWriter *document,
            /* [in] */ ULONG32 spCount,
            /* [size_is][in] */ ULONG32 offsets[  ],
            /* [size_is][in] */ ULONG32 lines[  ],
            /* [size_is][in] */ ULONG32 columns[  ],
            /* [size_is][in] */ ULONG32 endLines[  ],
            /* [size_is][in] */ ULONG32 endColumns[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE RemapToken( 
            /* [in] */ mdToken oldToken,
            /* [in] */ mdToken newToken) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE Initialize2( 
            /* [in] */ IUnknown *emitter,
            /* [in] */ const WCHAR *tempfilename,
            /* [in] */ IStream *pIStream,
            /* [in] */ BOOL fFullBuild,
            /* [in] */ const WCHAR *finalfilename) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE DefineConstant( 
            /* [in] */ const WCHAR *name,
            /* [in] */ VARIANT value,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ]) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE Abort( void) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedWriterVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedWriter * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedWriter * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedWriter * This);
        
        HRESULT ( STDMETHODCALLTYPE *DefineDocument )( 
            ISymUnmanagedWriter * This,
            /* [in] */ const WCHAR *url,
            /* [in] */ const GUID *language,
            /* [in] */ const GUID *languageVendor,
            /* [in] */ const GUID *documentType,
            /* [retval][out] */ ISymUnmanagedDocumentWriter **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *SetUserEntryPoint )( 
            ISymUnmanagedWriter * This,
            /* [in] */ mdMethodDef entryMethod);
        
        HRESULT ( STDMETHODCALLTYPE *OpenMethod )( 
            ISymUnmanagedWriter * This,
            /* [in] */ mdMethodDef method);
        
        HRESULT ( STDMETHODCALLTYPE *CloseMethod )( 
            ISymUnmanagedWriter * This);
        
        HRESULT ( STDMETHODCALLTYPE *OpenScope )( 
            ISymUnmanagedWriter * This,
            /* [in] */ ULONG32 startOffset,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *CloseScope )( 
            ISymUnmanagedWriter * This,
            /* [in] */ ULONG32 endOffset);
        
        HRESULT ( STDMETHODCALLTYPE *SetScopeRange )( 
            ISymUnmanagedWriter * This,
            /* [in] */ ULONG32 scopeID,
            /* [in] */ ULONG32 startOffset,
            /* [in] */ ULONG32 endOffset);
        
        HRESULT ( STDMETHODCALLTYPE *DefineLocalVariable )( 
            ISymUnmanagedWriter * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ],
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3,
            /* [in] */ ULONG32 startOffset,
            /* [in] */ ULONG32 endOffset);
        
        HRESULT ( STDMETHODCALLTYPE *DefineParameter )( 
            ISymUnmanagedWriter * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 sequence,
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3);
        
        HRESULT ( STDMETHODCALLTYPE *DefineField )( 
            ISymUnmanagedWriter * This,
            /* [in] */ mdTypeDef parent,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ],
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3);
        
        HRESULT ( STDMETHODCALLTYPE *DefineGlobalVariable )( 
            ISymUnmanagedWriter * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ],
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3);
        
        HRESULT ( STDMETHODCALLTYPE *Close )( 
            ISymUnmanagedWriter * This);
        
        HRESULT ( STDMETHODCALLTYPE *SetSymAttribute )( 
            ISymUnmanagedWriter * This,
            /* [in] */ mdToken parent,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 cData,
            /* [size_is][in] */ unsigned char data[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *OpenNamespace )( 
            ISymUnmanagedWriter * This,
            /* [in] */ const WCHAR *name);
        
        HRESULT ( STDMETHODCALLTYPE *CloseNamespace )( 
            ISymUnmanagedWriter * This);
        
        HRESULT ( STDMETHODCALLTYPE *UsingNamespace )( 
            ISymUnmanagedWriter * This,
            /* [in] */ const WCHAR *fullName);
        
        HRESULT ( STDMETHODCALLTYPE *SetMethodSourceRange )( 
            ISymUnmanagedWriter * This,
            /* [in] */ ISymUnmanagedDocumentWriter *startDoc,
            /* [in] */ ULONG32 startLine,
            /* [in] */ ULONG32 startColumn,
            /* [in] */ ISymUnmanagedDocumentWriter *endDoc,
            /* [in] */ ULONG32 endLine,
            /* [in] */ ULONG32 endColumn);
        
        HRESULT ( STDMETHODCALLTYPE *Initialize )( 
            ISymUnmanagedWriter * This,
            /* [in] */ IUnknown *emitter,
            /* [in] */ const WCHAR *filename,
            /* [in] */ IStream *pIStream,
            /* [in] */ BOOL fFullBuild);
        
        HRESULT ( STDMETHODCALLTYPE *GetDebugInfo )( 
            ISymUnmanagedWriter * This,
            /* [out][in] */ IMAGE_DEBUG_DIRECTORY *pIDD,
            /* [in] */ DWORD cData,
            /* [out] */ DWORD *pcData,
            /* [length_is][size_is][out] */ BYTE data[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *DefineSequencePoints )( 
            ISymUnmanagedWriter * This,
            /* [in] */ ISymUnmanagedDocumentWriter *document,
            /* [in] */ ULONG32 spCount,
            /* [size_is][in] */ ULONG32 offsets[  ],
            /* [size_is][in] */ ULONG32 lines[  ],
            /* [size_is][in] */ ULONG32 columns[  ],
            /* [size_is][in] */ ULONG32 endLines[  ],
            /* [size_is][in] */ ULONG32 endColumns[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *RemapToken )( 
            ISymUnmanagedWriter * This,
            /* [in] */ mdToken oldToken,
            /* [in] */ mdToken newToken);
        
        HRESULT ( STDMETHODCALLTYPE *Initialize2 )( 
            ISymUnmanagedWriter * This,
            /* [in] */ IUnknown *emitter,
            /* [in] */ const WCHAR *tempfilename,
            /* [in] */ IStream *pIStream,
            /* [in] */ BOOL fFullBuild,
            /* [in] */ const WCHAR *finalfilename);
        
        HRESULT ( STDMETHODCALLTYPE *DefineConstant )( 
            ISymUnmanagedWriter * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ VARIANT value,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *Abort )( 
            ISymUnmanagedWriter * This);
        
        END_INTERFACE
    } ISymUnmanagedWriterVtbl;

    interface ISymUnmanagedWriter
    {
        CONST_VTBL struct ISymUnmanagedWriterVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedWriter_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedWriter_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedWriter_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedWriter_DefineDocument(This,url,language,languageVendor,documentType,pRetVal)	\
    (This)->lpVtbl -> DefineDocument(This,url,language,languageVendor,documentType,pRetVal)

#define ISymUnmanagedWriter_SetUserEntryPoint(This,entryMethod)	\
    (This)->lpVtbl -> SetUserEntryPoint(This,entryMethod)

#define ISymUnmanagedWriter_OpenMethod(This,method)	\
    (This)->lpVtbl -> OpenMethod(This,method)

#define ISymUnmanagedWriter_CloseMethod(This)	\
    (This)->lpVtbl -> CloseMethod(This)

#define ISymUnmanagedWriter_OpenScope(This,startOffset,pRetVal)	\
    (This)->lpVtbl -> OpenScope(This,startOffset,pRetVal)

#define ISymUnmanagedWriter_CloseScope(This,endOffset)	\
    (This)->lpVtbl -> CloseScope(This,endOffset)

#define ISymUnmanagedWriter_SetScopeRange(This,scopeID,startOffset,endOffset)	\
    (This)->lpVtbl -> SetScopeRange(This,scopeID,startOffset,endOffset)

#define ISymUnmanagedWriter_DefineLocalVariable(This,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3,startOffset,endOffset)	\
    (This)->lpVtbl -> DefineLocalVariable(This,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3,startOffset,endOffset)

#define ISymUnmanagedWriter_DefineParameter(This,name,attributes,sequence,addrKind,addr1,addr2,addr3)	\
    (This)->lpVtbl -> DefineParameter(This,name,attributes,sequence,addrKind,addr1,addr2,addr3)

#define ISymUnmanagedWriter_DefineField(This,parent,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3)	\
    (This)->lpVtbl -> DefineField(This,parent,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3)

#define ISymUnmanagedWriter_DefineGlobalVariable(This,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3)	\
    (This)->lpVtbl -> DefineGlobalVariable(This,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3)

#define ISymUnmanagedWriter_Close(This)	\
    (This)->lpVtbl -> Close(This)

#define ISymUnmanagedWriter_SetSymAttribute(This,parent,name,cData,data)	\
    (This)->lpVtbl -> SetSymAttribute(This,parent,name,cData,data)

#define ISymUnmanagedWriter_OpenNamespace(This,name)	\
    (This)->lpVtbl -> OpenNamespace(This,name)

#define ISymUnmanagedWriter_CloseNamespace(This)	\
    (This)->lpVtbl -> CloseNamespace(This)

#define ISymUnmanagedWriter_UsingNamespace(This,fullName)	\
    (This)->lpVtbl -> UsingNamespace(This,fullName)

#define ISymUnmanagedWriter_SetMethodSourceRange(This,startDoc,startLine,startColumn,endDoc,endLine,endColumn)	\
    (This)->lpVtbl -> SetMethodSourceRange(This,startDoc,startLine,startColumn,endDoc,endLine,endColumn)

#define ISymUnmanagedWriter_Initialize(This,emitter,filename,pIStream,fFullBuild)	\
    (This)->lpVtbl -> Initialize(This,emitter,filename,pIStream,fFullBuild)

#define ISymUnmanagedWriter_GetDebugInfo(This,pIDD,cData,pcData,data)	\
    (This)->lpVtbl -> GetDebugInfo(This,pIDD,cData,pcData,data)

#define ISymUnmanagedWriter_DefineSequencePoints(This,document,spCount,offsets,lines,columns,endLines,endColumns)	\
    (This)->lpVtbl -> DefineSequencePoints(This,document,spCount,offsets,lines,columns,endLines,endColumns)

#define ISymUnmanagedWriter_RemapToken(This,oldToken,newToken)	\
    (This)->lpVtbl -> RemapToken(This,oldToken,newToken)

#define ISymUnmanagedWriter_Initialize2(This,emitter,tempfilename,pIStream,fFullBuild,finalfilename)	\
    (This)->lpVtbl -> Initialize2(This,emitter,tempfilename,pIStream,fFullBuild,finalfilename)

#define ISymUnmanagedWriter_DefineConstant(This,name,value,cSig,signature)	\
    (This)->lpVtbl -> DefineConstant(This,name,value,cSig,signature)

#define ISymUnmanagedWriter_Abort(This)	\
    (This)->lpVtbl -> Abort(This)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_DefineDocument_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ const WCHAR *url,
    /* [in] */ const GUID *language,
    /* [in] */ const GUID *languageVendor,
    /* [in] */ const GUID *documentType,
    /* [retval][out] */ ISymUnmanagedDocumentWriter **pRetVal);


void __RPC_STUB ISymUnmanagedWriter_DefineDocument_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_SetUserEntryPoint_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ mdMethodDef entryMethod);


void __RPC_STUB ISymUnmanagedWriter_SetUserEntryPoint_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_OpenMethod_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ mdMethodDef method);


void __RPC_STUB ISymUnmanagedWriter_OpenMethod_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_CloseMethod_Proxy( 
    ISymUnmanagedWriter * This);


void __RPC_STUB ISymUnmanagedWriter_CloseMethod_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_OpenScope_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ ULONG32 startOffset,
    /* [retval][out] */ ULONG32 *pRetVal);


void __RPC_STUB ISymUnmanagedWriter_OpenScope_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_CloseScope_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ ULONG32 endOffset);


void __RPC_STUB ISymUnmanagedWriter_CloseScope_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_SetScopeRange_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ ULONG32 scopeID,
    /* [in] */ ULONG32 startOffset,
    /* [in] */ ULONG32 endOffset);


void __RPC_STUB ISymUnmanagedWriter_SetScopeRange_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_DefineLocalVariable_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ const WCHAR *name,
    /* [in] */ ULONG32 attributes,
    /* [in] */ ULONG32 cSig,
    /* [size_is][in] */ unsigned char signature[  ],
    /* [in] */ ULONG32 addrKind,
    /* [in] */ ULONG32 addr1,
    /* [in] */ ULONG32 addr2,
    /* [in] */ ULONG32 addr3,
    /* [in] */ ULONG32 startOffset,
    /* [in] */ ULONG32 endOffset);


void __RPC_STUB ISymUnmanagedWriter_DefineLocalVariable_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_DefineParameter_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ const WCHAR *name,
    /* [in] */ ULONG32 attributes,
    /* [in] */ ULONG32 sequence,
    /* [in] */ ULONG32 addrKind,
    /* [in] */ ULONG32 addr1,
    /* [in] */ ULONG32 addr2,
    /* [in] */ ULONG32 addr3);


void __RPC_STUB ISymUnmanagedWriter_DefineParameter_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_DefineField_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ mdTypeDef parent,
    /* [in] */ const WCHAR *name,
    /* [in] */ ULONG32 attributes,
    /* [in] */ ULONG32 cSig,
    /* [size_is][in] */ unsigned char signature[  ],
    /* [in] */ ULONG32 addrKind,
    /* [in] */ ULONG32 addr1,
    /* [in] */ ULONG32 addr2,
    /* [in] */ ULONG32 addr3);


void __RPC_STUB ISymUnmanagedWriter_DefineField_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_DefineGlobalVariable_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ const WCHAR *name,
    /* [in] */ ULONG32 attributes,
    /* [in] */ ULONG32 cSig,
    /* [size_is][in] */ unsigned char signature[  ],
    /* [in] */ ULONG32 addrKind,
    /* [in] */ ULONG32 addr1,
    /* [in] */ ULONG32 addr2,
    /* [in] */ ULONG32 addr3);


void __RPC_STUB ISymUnmanagedWriter_DefineGlobalVariable_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_Close_Proxy( 
    ISymUnmanagedWriter * This);


void __RPC_STUB ISymUnmanagedWriter_Close_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_SetSymAttribute_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ mdToken parent,
    /* [in] */ const WCHAR *name,
    /* [in] */ ULONG32 cData,
    /* [size_is][in] */ unsigned char data[  ]);


void __RPC_STUB ISymUnmanagedWriter_SetSymAttribute_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_OpenNamespace_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ const WCHAR *name);


void __RPC_STUB ISymUnmanagedWriter_OpenNamespace_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_CloseNamespace_Proxy( 
    ISymUnmanagedWriter * This);


void __RPC_STUB ISymUnmanagedWriter_CloseNamespace_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_UsingNamespace_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ const WCHAR *fullName);


void __RPC_STUB ISymUnmanagedWriter_UsingNamespace_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_SetMethodSourceRange_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ ISymUnmanagedDocumentWriter *startDoc,
    /* [in] */ ULONG32 startLine,
    /* [in] */ ULONG32 startColumn,
    /* [in] */ ISymUnmanagedDocumentWriter *endDoc,
    /* [in] */ ULONG32 endLine,
    /* [in] */ ULONG32 endColumn);


void __RPC_STUB ISymUnmanagedWriter_SetMethodSourceRange_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_Initialize_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ IUnknown *emitter,
    /* [in] */ const WCHAR *filename,
    /* [in] */ IStream *pIStream,
    /* [in] */ BOOL fFullBuild);


void __RPC_STUB ISymUnmanagedWriter_Initialize_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_GetDebugInfo_Proxy( 
    ISymUnmanagedWriter * This,
    /* [out][in] */ IMAGE_DEBUG_DIRECTORY *pIDD,
    /* [in] */ DWORD cData,
    /* [out] */ DWORD *pcData,
    /* [length_is][size_is][out] */ BYTE data[  ]);


void __RPC_STUB ISymUnmanagedWriter_GetDebugInfo_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_DefineSequencePoints_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ ISymUnmanagedDocumentWriter *document,
    /* [in] */ ULONG32 spCount,
    /* [size_is][in] */ ULONG32 offsets[  ],
    /* [size_is][in] */ ULONG32 lines[  ],
    /* [size_is][in] */ ULONG32 columns[  ],
    /* [size_is][in] */ ULONG32 endLines[  ],
    /* [size_is][in] */ ULONG32 endColumns[  ]);


void __RPC_STUB ISymUnmanagedWriter_DefineSequencePoints_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_RemapToken_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ mdToken oldToken,
    /* [in] */ mdToken newToken);


void __RPC_STUB ISymUnmanagedWriter_RemapToken_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_Initialize2_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ IUnknown *emitter,
    /* [in] */ const WCHAR *tempfilename,
    /* [in] */ IStream *pIStream,
    /* [in] */ BOOL fFullBuild,
    /* [in] */ const WCHAR *finalfilename);


void __RPC_STUB ISymUnmanagedWriter_Initialize2_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_DefineConstant_Proxy( 
    ISymUnmanagedWriter * This,
    /* [in] */ const WCHAR *name,
    /* [in] */ VARIANT value,
    /* [in] */ ULONG32 cSig,
    /* [size_is][in] */ unsigned char signature[  ]);


void __RPC_STUB ISymUnmanagedWriter_DefineConstant_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter_Abort_Proxy( 
    ISymUnmanagedWriter * This);


void __RPC_STUB ISymUnmanagedWriter_Abort_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedWriter_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedWriter2_INTERFACE_DEFINED__
#define __ISymUnmanagedWriter2_INTERFACE_DEFINED__

/* interface ISymUnmanagedWriter2 */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedWriter2;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("0B97726E-9E6D-4f05-9A26-424022093CAA")
    ISymUnmanagedWriter2 : public ISymUnmanagedWriter
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE DefineLocalVariable2( 
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ mdSignature sigToken,
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3,
            /* [in] */ ULONG32 startOffset,
            /* [in] */ ULONG32 endOffset) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE DefineGlobalVariable2( 
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ mdSignature sigToken,
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE DefineConstant2( 
            /* [in] */ const WCHAR *name,
            /* [in] */ VARIANT value,
            /* [in] */ mdSignature sigToken) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedWriter2Vtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedWriter2 * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedWriter2 * This);
        
        HRESULT ( STDMETHODCALLTYPE *DefineDocument )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ const WCHAR *url,
            /* [in] */ const GUID *language,
            /* [in] */ const GUID *languageVendor,
            /* [in] */ const GUID *documentType,
            /* [retval][out] */ ISymUnmanagedDocumentWriter **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *SetUserEntryPoint )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ mdMethodDef entryMethod);
        
        HRESULT ( STDMETHODCALLTYPE *OpenMethod )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ mdMethodDef method);
        
        HRESULT ( STDMETHODCALLTYPE *CloseMethod )( 
            ISymUnmanagedWriter2 * This);
        
        HRESULT ( STDMETHODCALLTYPE *OpenScope )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ ULONG32 startOffset,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *CloseScope )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ ULONG32 endOffset);
        
        HRESULT ( STDMETHODCALLTYPE *SetScopeRange )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ ULONG32 scopeID,
            /* [in] */ ULONG32 startOffset,
            /* [in] */ ULONG32 endOffset);
        
        HRESULT ( STDMETHODCALLTYPE *DefineLocalVariable )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ],
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3,
            /* [in] */ ULONG32 startOffset,
            /* [in] */ ULONG32 endOffset);
        
        HRESULT ( STDMETHODCALLTYPE *DefineParameter )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 sequence,
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3);
        
        HRESULT ( STDMETHODCALLTYPE *DefineField )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ mdTypeDef parent,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ],
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3);
        
        HRESULT ( STDMETHODCALLTYPE *DefineGlobalVariable )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ],
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3);
        
        HRESULT ( STDMETHODCALLTYPE *Close )( 
            ISymUnmanagedWriter2 * This);
        
        HRESULT ( STDMETHODCALLTYPE *SetSymAttribute )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ mdToken parent,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 cData,
            /* [size_is][in] */ unsigned char data[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *OpenNamespace )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ const WCHAR *name);
        
        HRESULT ( STDMETHODCALLTYPE *CloseNamespace )( 
            ISymUnmanagedWriter2 * This);
        
        HRESULT ( STDMETHODCALLTYPE *UsingNamespace )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ const WCHAR *fullName);
        
        HRESULT ( STDMETHODCALLTYPE *SetMethodSourceRange )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ ISymUnmanagedDocumentWriter *startDoc,
            /* [in] */ ULONG32 startLine,
            /* [in] */ ULONG32 startColumn,
            /* [in] */ ISymUnmanagedDocumentWriter *endDoc,
            /* [in] */ ULONG32 endLine,
            /* [in] */ ULONG32 endColumn);
        
        HRESULT ( STDMETHODCALLTYPE *Initialize )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ IUnknown *emitter,
            /* [in] */ const WCHAR *filename,
            /* [in] */ IStream *pIStream,
            /* [in] */ BOOL fFullBuild);
        
        HRESULT ( STDMETHODCALLTYPE *GetDebugInfo )( 
            ISymUnmanagedWriter2 * This,
            /* [out][in] */ IMAGE_DEBUG_DIRECTORY *pIDD,
            /* [in] */ DWORD cData,
            /* [out] */ DWORD *pcData,
            /* [length_is][size_is][out] */ BYTE data[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *DefineSequencePoints )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ ISymUnmanagedDocumentWriter *document,
            /* [in] */ ULONG32 spCount,
            /* [size_is][in] */ ULONG32 offsets[  ],
            /* [size_is][in] */ ULONG32 lines[  ],
            /* [size_is][in] */ ULONG32 columns[  ],
            /* [size_is][in] */ ULONG32 endLines[  ],
            /* [size_is][in] */ ULONG32 endColumns[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *RemapToken )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ mdToken oldToken,
            /* [in] */ mdToken newToken);
        
        HRESULT ( STDMETHODCALLTYPE *Initialize2 )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ IUnknown *emitter,
            /* [in] */ const WCHAR *tempfilename,
            /* [in] */ IStream *pIStream,
            /* [in] */ BOOL fFullBuild,
            /* [in] */ const WCHAR *finalfilename);
        
        HRESULT ( STDMETHODCALLTYPE *DefineConstant )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ VARIANT value,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *Abort )( 
            ISymUnmanagedWriter2 * This);
        
        HRESULT ( STDMETHODCALLTYPE *DefineLocalVariable2 )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ mdSignature sigToken,
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3,
            /* [in] */ ULONG32 startOffset,
            /* [in] */ ULONG32 endOffset);
        
        HRESULT ( STDMETHODCALLTYPE *DefineGlobalVariable2 )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ mdSignature sigToken,
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3);
        
        HRESULT ( STDMETHODCALLTYPE *DefineConstant2 )( 
            ISymUnmanagedWriter2 * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ VARIANT value,
            /* [in] */ mdSignature sigToken);
        
        END_INTERFACE
    } ISymUnmanagedWriter2Vtbl;

    interface ISymUnmanagedWriter2
    {
        CONST_VTBL struct ISymUnmanagedWriter2Vtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedWriter2_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedWriter2_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedWriter2_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedWriter2_DefineDocument(This,url,language,languageVendor,documentType,pRetVal)	\
    (This)->lpVtbl -> DefineDocument(This,url,language,languageVendor,documentType,pRetVal)

#define ISymUnmanagedWriter2_SetUserEntryPoint(This,entryMethod)	\
    (This)->lpVtbl -> SetUserEntryPoint(This,entryMethod)

#define ISymUnmanagedWriter2_OpenMethod(This,method)	\
    (This)->lpVtbl -> OpenMethod(This,method)

#define ISymUnmanagedWriter2_CloseMethod(This)	\
    (This)->lpVtbl -> CloseMethod(This)

#define ISymUnmanagedWriter2_OpenScope(This,startOffset,pRetVal)	\
    (This)->lpVtbl -> OpenScope(This,startOffset,pRetVal)

#define ISymUnmanagedWriter2_CloseScope(This,endOffset)	\
    (This)->lpVtbl -> CloseScope(This,endOffset)

#define ISymUnmanagedWriter2_SetScopeRange(This,scopeID,startOffset,endOffset)	\
    (This)->lpVtbl -> SetScopeRange(This,scopeID,startOffset,endOffset)

#define ISymUnmanagedWriter2_DefineLocalVariable(This,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3,startOffset,endOffset)	\
    (This)->lpVtbl -> DefineLocalVariable(This,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3,startOffset,endOffset)

#define ISymUnmanagedWriter2_DefineParameter(This,name,attributes,sequence,addrKind,addr1,addr2,addr3)	\
    (This)->lpVtbl -> DefineParameter(This,name,attributes,sequence,addrKind,addr1,addr2,addr3)

#define ISymUnmanagedWriter2_DefineField(This,parent,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3)	\
    (This)->lpVtbl -> DefineField(This,parent,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3)

#define ISymUnmanagedWriter2_DefineGlobalVariable(This,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3)	\
    (This)->lpVtbl -> DefineGlobalVariable(This,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3)

#define ISymUnmanagedWriter2_Close(This)	\
    (This)->lpVtbl -> Close(This)

#define ISymUnmanagedWriter2_SetSymAttribute(This,parent,name,cData,data)	\
    (This)->lpVtbl -> SetSymAttribute(This,parent,name,cData,data)

#define ISymUnmanagedWriter2_OpenNamespace(This,name)	\
    (This)->lpVtbl -> OpenNamespace(This,name)

#define ISymUnmanagedWriter2_CloseNamespace(This)	\
    (This)->lpVtbl -> CloseNamespace(This)

#define ISymUnmanagedWriter2_UsingNamespace(This,fullName)	\
    (This)->lpVtbl -> UsingNamespace(This,fullName)

#define ISymUnmanagedWriter2_SetMethodSourceRange(This,startDoc,startLine,startColumn,endDoc,endLine,endColumn)	\
    (This)->lpVtbl -> SetMethodSourceRange(This,startDoc,startLine,startColumn,endDoc,endLine,endColumn)

#define ISymUnmanagedWriter2_Initialize(This,emitter,filename,pIStream,fFullBuild)	\
    (This)->lpVtbl -> Initialize(This,emitter,filename,pIStream,fFullBuild)

#define ISymUnmanagedWriter2_GetDebugInfo(This,pIDD,cData,pcData,data)	\
    (This)->lpVtbl -> GetDebugInfo(This,pIDD,cData,pcData,data)

#define ISymUnmanagedWriter2_DefineSequencePoints(This,document,spCount,offsets,lines,columns,endLines,endColumns)	\
    (This)->lpVtbl -> DefineSequencePoints(This,document,spCount,offsets,lines,columns,endLines,endColumns)

#define ISymUnmanagedWriter2_RemapToken(This,oldToken,newToken)	\
    (This)->lpVtbl -> RemapToken(This,oldToken,newToken)

#define ISymUnmanagedWriter2_Initialize2(This,emitter,tempfilename,pIStream,fFullBuild,finalfilename)	\
    (This)->lpVtbl -> Initialize2(This,emitter,tempfilename,pIStream,fFullBuild,finalfilename)

#define ISymUnmanagedWriter2_DefineConstant(This,name,value,cSig,signature)	\
    (This)->lpVtbl -> DefineConstant(This,name,value,cSig,signature)

#define ISymUnmanagedWriter2_Abort(This)	\
    (This)->lpVtbl -> Abort(This)


#define ISymUnmanagedWriter2_DefineLocalVariable2(This,name,attributes,sigToken,addrKind,addr1,addr2,addr3,startOffset,endOffset)	\
    (This)->lpVtbl -> DefineLocalVariable2(This,name,attributes,sigToken,addrKind,addr1,addr2,addr3,startOffset,endOffset)

#define ISymUnmanagedWriter2_DefineGlobalVariable2(This,name,attributes,sigToken,addrKind,addr1,addr2,addr3)	\
    (This)->lpVtbl -> DefineGlobalVariable2(This,name,attributes,sigToken,addrKind,addr1,addr2,addr3)

#define ISymUnmanagedWriter2_DefineConstant2(This,name,value,sigToken)	\
    (This)->lpVtbl -> DefineConstant2(This,name,value,sigToken)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter2_DefineLocalVariable2_Proxy( 
    ISymUnmanagedWriter2 * This,
    /* [in] */ const WCHAR *name,
    /* [in] */ ULONG32 attributes,
    /* [in] */ mdSignature sigToken,
    /* [in] */ ULONG32 addrKind,
    /* [in] */ ULONG32 addr1,
    /* [in] */ ULONG32 addr2,
    /* [in] */ ULONG32 addr3,
    /* [in] */ ULONG32 startOffset,
    /* [in] */ ULONG32 endOffset);


void __RPC_STUB ISymUnmanagedWriter2_DefineLocalVariable2_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter2_DefineGlobalVariable2_Proxy( 
    ISymUnmanagedWriter2 * This,
    /* [in] */ const WCHAR *name,
    /* [in] */ ULONG32 attributes,
    /* [in] */ mdSignature sigToken,
    /* [in] */ ULONG32 addrKind,
    /* [in] */ ULONG32 addr1,
    /* [in] */ ULONG32 addr2,
    /* [in] */ ULONG32 addr3);


void __RPC_STUB ISymUnmanagedWriter2_DefineGlobalVariable2_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter2_DefineConstant2_Proxy( 
    ISymUnmanagedWriter2 * This,
    /* [in] */ const WCHAR *name,
    /* [in] */ VARIANT value,
    /* [in] */ mdSignature sigToken);


void __RPC_STUB ISymUnmanagedWriter2_DefineConstant2_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedWriter2_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedWriter3_INTERFACE_DEFINED__
#define __ISymUnmanagedWriter3_INTERFACE_DEFINED__

/* interface ISymUnmanagedWriter3 */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedWriter3;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("12F1E02C-1E05-4B0E-9468-EBC9D1BB040F")
    ISymUnmanagedWriter3 : public ISymUnmanagedWriter2
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE OpenMethod2( 
            /* [in] */ mdMethodDef method,
            /* [in] */ ULONG32 isect,
            /* [in] */ ULONG32 offset) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE Commit( void) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedWriter3Vtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedWriter3 * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedWriter3 * This);
        
        HRESULT ( STDMETHODCALLTYPE *DefineDocument )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ const WCHAR *url,
            /* [in] */ const GUID *language,
            /* [in] */ const GUID *languageVendor,
            /* [in] */ const GUID *documentType,
            /* [retval][out] */ ISymUnmanagedDocumentWriter **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *SetUserEntryPoint )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ mdMethodDef entryMethod);
        
        HRESULT ( STDMETHODCALLTYPE *OpenMethod )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ mdMethodDef method);
        
        HRESULT ( STDMETHODCALLTYPE *CloseMethod )( 
            ISymUnmanagedWriter3 * This);
        
        HRESULT ( STDMETHODCALLTYPE *OpenScope )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ ULONG32 startOffset,
            /* [retval][out] */ ULONG32 *pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *CloseScope )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ ULONG32 endOffset);
        
        HRESULT ( STDMETHODCALLTYPE *SetScopeRange )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ ULONG32 scopeID,
            /* [in] */ ULONG32 startOffset,
            /* [in] */ ULONG32 endOffset);
        
        HRESULT ( STDMETHODCALLTYPE *DefineLocalVariable )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ],
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3,
            /* [in] */ ULONG32 startOffset,
            /* [in] */ ULONG32 endOffset);
        
        HRESULT ( STDMETHODCALLTYPE *DefineParameter )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 sequence,
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3);
        
        HRESULT ( STDMETHODCALLTYPE *DefineField )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ mdTypeDef parent,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ],
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3);
        
        HRESULT ( STDMETHODCALLTYPE *DefineGlobalVariable )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ],
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3);
        
        HRESULT ( STDMETHODCALLTYPE *Close )( 
            ISymUnmanagedWriter3 * This);
        
        HRESULT ( STDMETHODCALLTYPE *SetSymAttribute )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ mdToken parent,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 cData,
            /* [size_is][in] */ unsigned char data[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *OpenNamespace )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ const WCHAR *name);
        
        HRESULT ( STDMETHODCALLTYPE *CloseNamespace )( 
            ISymUnmanagedWriter3 * This);
        
        HRESULT ( STDMETHODCALLTYPE *UsingNamespace )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ const WCHAR *fullName);
        
        HRESULT ( STDMETHODCALLTYPE *SetMethodSourceRange )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ ISymUnmanagedDocumentWriter *startDoc,
            /* [in] */ ULONG32 startLine,
            /* [in] */ ULONG32 startColumn,
            /* [in] */ ISymUnmanagedDocumentWriter *endDoc,
            /* [in] */ ULONG32 endLine,
            /* [in] */ ULONG32 endColumn);
        
        HRESULT ( STDMETHODCALLTYPE *Initialize )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ IUnknown *emitter,
            /* [in] */ const WCHAR *filename,
            /* [in] */ IStream *pIStream,
            /* [in] */ BOOL fFullBuild);
        
        HRESULT ( STDMETHODCALLTYPE *GetDebugInfo )( 
            ISymUnmanagedWriter3 * This,
            /* [out][in] */ IMAGE_DEBUG_DIRECTORY *pIDD,
            /* [in] */ DWORD cData,
            /* [out] */ DWORD *pcData,
            /* [length_is][size_is][out] */ BYTE data[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *DefineSequencePoints )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ ISymUnmanagedDocumentWriter *document,
            /* [in] */ ULONG32 spCount,
            /* [size_is][in] */ ULONG32 offsets[  ],
            /* [size_is][in] */ ULONG32 lines[  ],
            /* [size_is][in] */ ULONG32 columns[  ],
            /* [size_is][in] */ ULONG32 endLines[  ],
            /* [size_is][in] */ ULONG32 endColumns[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *RemapToken )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ mdToken oldToken,
            /* [in] */ mdToken newToken);
        
        HRESULT ( STDMETHODCALLTYPE *Initialize2 )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ IUnknown *emitter,
            /* [in] */ const WCHAR *tempfilename,
            /* [in] */ IStream *pIStream,
            /* [in] */ BOOL fFullBuild,
            /* [in] */ const WCHAR *finalfilename);
        
        HRESULT ( STDMETHODCALLTYPE *DefineConstant )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ VARIANT value,
            /* [in] */ ULONG32 cSig,
            /* [size_is][in] */ unsigned char signature[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *Abort )( 
            ISymUnmanagedWriter3 * This);
        
        HRESULT ( STDMETHODCALLTYPE *DefineLocalVariable2 )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ mdSignature sigToken,
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3,
            /* [in] */ ULONG32 startOffset,
            /* [in] */ ULONG32 endOffset);
        
        HRESULT ( STDMETHODCALLTYPE *DefineGlobalVariable2 )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ ULONG32 attributes,
            /* [in] */ mdSignature sigToken,
            /* [in] */ ULONG32 addrKind,
            /* [in] */ ULONG32 addr1,
            /* [in] */ ULONG32 addr2,
            /* [in] */ ULONG32 addr3);
        
        HRESULT ( STDMETHODCALLTYPE *DefineConstant2 )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ const WCHAR *name,
            /* [in] */ VARIANT value,
            /* [in] */ mdSignature sigToken);
        
        HRESULT ( STDMETHODCALLTYPE *OpenMethod2 )( 
            ISymUnmanagedWriter3 * This,
            /* [in] */ mdMethodDef method,
            /* [in] */ ULONG32 isect,
            /* [in] */ ULONG32 offset);
        
        HRESULT ( STDMETHODCALLTYPE *Commit )( 
            ISymUnmanagedWriter3 * This);
        
        END_INTERFACE
    } ISymUnmanagedWriter3Vtbl;

    interface ISymUnmanagedWriter3
    {
        CONST_VTBL struct ISymUnmanagedWriter3Vtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedWriter3_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedWriter3_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedWriter3_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedWriter3_DefineDocument(This,url,language,languageVendor,documentType,pRetVal)	\
    (This)->lpVtbl -> DefineDocument(This,url,language,languageVendor,documentType,pRetVal)

#define ISymUnmanagedWriter3_SetUserEntryPoint(This,entryMethod)	\
    (This)->lpVtbl -> SetUserEntryPoint(This,entryMethod)

#define ISymUnmanagedWriter3_OpenMethod(This,method)	\
    (This)->lpVtbl -> OpenMethod(This,method)

#define ISymUnmanagedWriter3_CloseMethod(This)	\
    (This)->lpVtbl -> CloseMethod(This)

#define ISymUnmanagedWriter3_OpenScope(This,startOffset,pRetVal)	\
    (This)->lpVtbl -> OpenScope(This,startOffset,pRetVal)

#define ISymUnmanagedWriter3_CloseScope(This,endOffset)	\
    (This)->lpVtbl -> CloseScope(This,endOffset)

#define ISymUnmanagedWriter3_SetScopeRange(This,scopeID,startOffset,endOffset)	\
    (This)->lpVtbl -> SetScopeRange(This,scopeID,startOffset,endOffset)

#define ISymUnmanagedWriter3_DefineLocalVariable(This,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3,startOffset,endOffset)	\
    (This)->lpVtbl -> DefineLocalVariable(This,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3,startOffset,endOffset)

#define ISymUnmanagedWriter3_DefineParameter(This,name,attributes,sequence,addrKind,addr1,addr2,addr3)	\
    (This)->lpVtbl -> DefineParameter(This,name,attributes,sequence,addrKind,addr1,addr2,addr3)

#define ISymUnmanagedWriter3_DefineField(This,parent,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3)	\
    (This)->lpVtbl -> DefineField(This,parent,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3)

#define ISymUnmanagedWriter3_DefineGlobalVariable(This,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3)	\
    (This)->lpVtbl -> DefineGlobalVariable(This,name,attributes,cSig,signature,addrKind,addr1,addr2,addr3)

#define ISymUnmanagedWriter3_Close(This)	\
    (This)->lpVtbl -> Close(This)

#define ISymUnmanagedWriter3_SetSymAttribute(This,parent,name,cData,data)	\
    (This)->lpVtbl -> SetSymAttribute(This,parent,name,cData,data)

#define ISymUnmanagedWriter3_OpenNamespace(This,name)	\
    (This)->lpVtbl -> OpenNamespace(This,name)

#define ISymUnmanagedWriter3_CloseNamespace(This)	\
    (This)->lpVtbl -> CloseNamespace(This)

#define ISymUnmanagedWriter3_UsingNamespace(This,fullName)	\
    (This)->lpVtbl -> UsingNamespace(This,fullName)

#define ISymUnmanagedWriter3_SetMethodSourceRange(This,startDoc,startLine,startColumn,endDoc,endLine,endColumn)	\
    (This)->lpVtbl -> SetMethodSourceRange(This,startDoc,startLine,startColumn,endDoc,endLine,endColumn)

#define ISymUnmanagedWriter3_Initialize(This,emitter,filename,pIStream,fFullBuild)	\
    (This)->lpVtbl -> Initialize(This,emitter,filename,pIStream,fFullBuild)

#define ISymUnmanagedWriter3_GetDebugInfo(This,pIDD,cData,pcData,data)	\
    (This)->lpVtbl -> GetDebugInfo(This,pIDD,cData,pcData,data)

#define ISymUnmanagedWriter3_DefineSequencePoints(This,document,spCount,offsets,lines,columns,endLines,endColumns)	\
    (This)->lpVtbl -> DefineSequencePoints(This,document,spCount,offsets,lines,columns,endLines,endColumns)

#define ISymUnmanagedWriter3_RemapToken(This,oldToken,newToken)	\
    (This)->lpVtbl -> RemapToken(This,oldToken,newToken)

#define ISymUnmanagedWriter3_Initialize2(This,emitter,tempfilename,pIStream,fFullBuild,finalfilename)	\
    (This)->lpVtbl -> Initialize2(This,emitter,tempfilename,pIStream,fFullBuild,finalfilename)

#define ISymUnmanagedWriter3_DefineConstant(This,name,value,cSig,signature)	\
    (This)->lpVtbl -> DefineConstant(This,name,value,cSig,signature)

#define ISymUnmanagedWriter3_Abort(This)	\
    (This)->lpVtbl -> Abort(This)


#define ISymUnmanagedWriter3_DefineLocalVariable2(This,name,attributes,sigToken,addrKind,addr1,addr2,addr3,startOffset,endOffset)	\
    (This)->lpVtbl -> DefineLocalVariable2(This,name,attributes,sigToken,addrKind,addr1,addr2,addr3,startOffset,endOffset)

#define ISymUnmanagedWriter3_DefineGlobalVariable2(This,name,attributes,sigToken,addrKind,addr1,addr2,addr3)	\
    (This)->lpVtbl -> DefineGlobalVariable2(This,name,attributes,sigToken,addrKind,addr1,addr2,addr3)

#define ISymUnmanagedWriter3_DefineConstant2(This,name,value,sigToken)	\
    (This)->lpVtbl -> DefineConstant2(This,name,value,sigToken)


#define ISymUnmanagedWriter3_OpenMethod2(This,method,isect,offset)	\
    (This)->lpVtbl -> OpenMethod2(This,method,isect,offset)

#define ISymUnmanagedWriter3_Commit(This)	\
    (This)->lpVtbl -> Commit(This)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter3_OpenMethod2_Proxy( 
    ISymUnmanagedWriter3 * This,
    /* [in] */ mdMethodDef method,
    /* [in] */ ULONG32 isect,
    /* [in] */ ULONG32 offset);


void __RPC_STUB ISymUnmanagedWriter3_OpenMethod2_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedWriter3_Commit_Proxy( 
    ISymUnmanagedWriter3 * This);


void __RPC_STUB ISymUnmanagedWriter3_Commit_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedWriter3_INTERFACE_DEFINED__ */


#ifndef __ISymUnmanagedReader2_INTERFACE_DEFINED__
#define __ISymUnmanagedReader2_INTERFACE_DEFINED__

/* interface ISymUnmanagedReader2 */
/* [unique][uuid][object] */ 


EXTERN_C const IID IID_ISymUnmanagedReader2;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("A09E53B2-2A57-4cca-8F63-B84F7C35D4AA")
    ISymUnmanagedReader2 : public ISymUnmanagedReader
    {
    public:
        virtual HRESULT STDMETHODCALLTYPE GetMethodByVersionPreRemap( 
            /* [in] */ mdMethodDef token,
            /* [in] */ int version,
            /* [retval][out] */ ISymUnmanagedMethod **pRetVal) = 0;
        
        virtual HRESULT STDMETHODCALLTYPE GetSymAttributePreRemap( 
            /* [in] */ mdToken parent,
            /* [in] */ WCHAR *name,
            /* [in] */ ULONG32 cBuffer,
            /* [out] */ ULONG32 *pcBuffer,
            /* [length_is][size_is][out] */ BYTE buffer[  ]) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct ISymUnmanagedReader2Vtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ REFIID riid,
            /* [iid_is][out] */ void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            ISymUnmanagedReader2 * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            ISymUnmanagedReader2 * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetDocument )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ WCHAR *url,
            /* [in] */ GUID language,
            /* [in] */ GUID languageVendor,
            /* [in] */ GUID documentType,
            /* [retval][out] */ ISymUnmanagedDocument **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetDocuments )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ ULONG32 cDocs,
            /* [out] */ ULONG32 *pcDocs,
            /* [length_is][size_is][out] */ ISymUnmanagedDocument *pDocs[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetUserEntryPoint )( 
            ISymUnmanagedReader2 * This,
            /* [retval][out] */ mdMethodDef *pToken);
        
        HRESULT ( STDMETHODCALLTYPE *GetMethod )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ mdMethodDef token,
            /* [retval][out] */ ISymUnmanagedMethod **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetMethodByVersion )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ mdMethodDef token,
            /* [in] */ int version,
            /* [retval][out] */ ISymUnmanagedMethod **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetVariables )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ mdToken parent,
            /* [in] */ ULONG32 cVars,
            /* [out] */ ULONG32 *pcVars,
            /* [length_is][size_is][out] */ ISymUnmanagedVariable *pVars[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetGlobalVariables )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ ULONG32 cVars,
            /* [out] */ ULONG32 *pcVars,
            /* [length_is][size_is][out] */ ISymUnmanagedVariable *pVars[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetMethodFromDocumentPosition )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ ISymUnmanagedDocument *document,
            /* [in] */ ULONG32 line,
            /* [in] */ ULONG32 column,
            /* [retval][out] */ ISymUnmanagedMethod **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetSymAttribute )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ mdToken parent,
            /* [in] */ WCHAR *name,
            /* [in] */ ULONG32 cBuffer,
            /* [out] */ ULONG32 *pcBuffer,
            /* [length_is][size_is][out] */ BYTE buffer[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetNamespaces )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ ULONG32 cNameSpaces,
            /* [out] */ ULONG32 *pcNameSpaces,
            /* [length_is][size_is][out] */ ISymUnmanagedNamespace *namespaces[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *Initialize )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ IUnknown *importer,
            /* [in] */ const WCHAR *filename,
            /* [in] */ const WCHAR *searchPath,
            /* [in] */ IStream *pIStream);
        
        HRESULT ( STDMETHODCALLTYPE *UpdateSymbolStore )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ const WCHAR *filename,
            /* [in] */ IStream *pIStream);
        
        HRESULT ( STDMETHODCALLTYPE *ReplaceSymbolStore )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ const WCHAR *filename,
            /* [in] */ IStream *pIStream);
        
        HRESULT ( STDMETHODCALLTYPE *GetSymbolStoreFileName )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ ULONG32 cchName,
            /* [out] */ ULONG32 *pcchName,
            /* [length_is][size_is][out] */ WCHAR szName[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetMethodsFromDocumentPosition )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ ISymUnmanagedDocument *document,
            /* [in] */ ULONG32 line,
            /* [in] */ ULONG32 column,
            /* [in] */ ULONG32 cMethod,
            /* [out] */ ULONG32 *pcMethod,
            /* [length_is][size_is][out] */ ISymUnmanagedMethod *pRetVal[  ]);
        
        HRESULT ( STDMETHODCALLTYPE *GetDocumentVersion )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ ISymUnmanagedDocument *pDoc,
            /* [out] */ int *version,
            /* [out] */ BOOL *pbCurrent);
        
        HRESULT ( STDMETHODCALLTYPE *GetMethodVersion )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ ISymUnmanagedMethod *pMethod,
            /* [out] */ int *version);
        
        HRESULT ( STDMETHODCALLTYPE *GetMethodByVersionPreRemap )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ mdMethodDef token,
            /* [in] */ int version,
            /* [retval][out] */ ISymUnmanagedMethod **pRetVal);
        
        HRESULT ( STDMETHODCALLTYPE *GetSymAttributePreRemap )( 
            ISymUnmanagedReader2 * This,
            /* [in] */ mdToken parent,
            /* [in] */ WCHAR *name,
            /* [in] */ ULONG32 cBuffer,
            /* [out] */ ULONG32 *pcBuffer,
            /* [length_is][size_is][out] */ BYTE buffer[  ]);
        
        END_INTERFACE
    } ISymUnmanagedReader2Vtbl;

    interface ISymUnmanagedReader2
    {
        CONST_VTBL struct ISymUnmanagedReader2Vtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define ISymUnmanagedReader2_QueryInterface(This,riid,ppvObject)	\
    (This)->lpVtbl -> QueryInterface(This,riid,ppvObject)

#define ISymUnmanagedReader2_AddRef(This)	\
    (This)->lpVtbl -> AddRef(This)

#define ISymUnmanagedReader2_Release(This)	\
    (This)->lpVtbl -> Release(This)


#define ISymUnmanagedReader2_GetDocument(This,url,language,languageVendor,documentType,pRetVal)	\
    (This)->lpVtbl -> GetDocument(This,url,language,languageVendor,documentType,pRetVal)

#define ISymUnmanagedReader2_GetDocuments(This,cDocs,pcDocs,pDocs)	\
    (This)->lpVtbl -> GetDocuments(This,cDocs,pcDocs,pDocs)

#define ISymUnmanagedReader2_GetUserEntryPoint(This,pToken)	\
    (This)->lpVtbl -> GetUserEntryPoint(This,pToken)

#define ISymUnmanagedReader2_GetMethod(This,token,pRetVal)	\
    (This)->lpVtbl -> GetMethod(This,token,pRetVal)

#define ISymUnmanagedReader2_GetMethodByVersion(This,token,version,pRetVal)	\
    (This)->lpVtbl -> GetMethodByVersion(This,token,version,pRetVal)

#define ISymUnmanagedReader2_GetVariables(This,parent,cVars,pcVars,pVars)	\
    (This)->lpVtbl -> GetVariables(This,parent,cVars,pcVars,pVars)

#define ISymUnmanagedReader2_GetGlobalVariables(This,cVars,pcVars,pVars)	\
    (This)->lpVtbl -> GetGlobalVariables(This,cVars,pcVars,pVars)

#define ISymUnmanagedReader2_GetMethodFromDocumentPosition(This,document,line,column,pRetVal)	\
    (This)->lpVtbl -> GetMethodFromDocumentPosition(This,document,line,column,pRetVal)

#define ISymUnmanagedReader2_GetSymAttribute(This,parent,name,cBuffer,pcBuffer,buffer)	\
    (This)->lpVtbl -> GetSymAttribute(This,parent,name,cBuffer,pcBuffer,buffer)

#define ISymUnmanagedReader2_GetNamespaces(This,cNameSpaces,pcNameSpaces,namespaces)	\
    (This)->lpVtbl -> GetNamespaces(This,cNameSpaces,pcNameSpaces,namespaces)

#define ISymUnmanagedReader2_Initialize(This,importer,filename,searchPath,pIStream)	\
    (This)->lpVtbl -> Initialize(This,importer,filename,searchPath,pIStream)

#define ISymUnmanagedReader2_UpdateSymbolStore(This,filename,pIStream)	\
    (This)->lpVtbl -> UpdateSymbolStore(This,filename,pIStream)

#define ISymUnmanagedReader2_ReplaceSymbolStore(This,filename,pIStream)	\
    (This)->lpVtbl -> ReplaceSymbolStore(This,filename,pIStream)

#define ISymUnmanagedReader2_GetSymbolStoreFileName(This,cchName,pcchName,szName)	\
    (This)->lpVtbl -> GetSymbolStoreFileName(This,cchName,pcchName,szName)

#define ISymUnmanagedReader2_GetMethodsFromDocumentPosition(This,document,line,column,cMethod,pcMethod,pRetVal)	\
    (This)->lpVtbl -> GetMethodsFromDocumentPosition(This,document,line,column,cMethod,pcMethod,pRetVal)

#define ISymUnmanagedReader2_GetDocumentVersion(This,pDoc,version,pbCurrent)	\
    (This)->lpVtbl -> GetDocumentVersion(This,pDoc,version,pbCurrent)

#define ISymUnmanagedReader2_GetMethodVersion(This,pMethod,version)	\
    (This)->lpVtbl -> GetMethodVersion(This,pMethod,version)


#define ISymUnmanagedReader2_GetMethodByVersionPreRemap(This,token,version,pRetVal)	\
    (This)->lpVtbl -> GetMethodByVersionPreRemap(This,token,version,pRetVal)

#define ISymUnmanagedReader2_GetSymAttributePreRemap(This,parent,name,cBuffer,pcBuffer,buffer)	\
    (This)->lpVtbl -> GetSymAttributePreRemap(This,parent,name,cBuffer,pcBuffer,buffer)

#endif /* COBJMACROS */


#endif 	/* C style interface */



HRESULT STDMETHODCALLTYPE ISymUnmanagedReader2_GetMethodByVersionPreRemap_Proxy( 
    ISymUnmanagedReader2 * This,
    /* [in] */ mdMethodDef token,
    /* [in] */ int version,
    /* [retval][out] */ ISymUnmanagedMethod **pRetVal);


void __RPC_STUB ISymUnmanagedReader2_GetMethodByVersionPreRemap_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);


HRESULT STDMETHODCALLTYPE ISymUnmanagedReader2_GetSymAttributePreRemap_Proxy( 
    ISymUnmanagedReader2 * This,
    /* [in] */ mdToken parent,
    /* [in] */ WCHAR *name,
    /* [in] */ ULONG32 cBuffer,
    /* [out] */ ULONG32 *pcBuffer,
    /* [length_is][size_is][out] */ BYTE buffer[  ]);


void __RPC_STUB ISymUnmanagedReader2_GetSymAttributePreRemap_Stub(
    IRpcStubBuffer *This,
    IRpcChannelBuffer *_pRpcChannelBuffer,
    PRPC_MESSAGE _pRpcMessage,
    DWORD *_pdwStubPhase);



#endif 	/* __ISymUnmanagedReader2_INTERFACE_DEFINED__ */


/* Additional Prototypes for ALL interfaces */

unsigned long             __RPC_USER  VARIANT_UserSize(     unsigned long *, unsigned long            , VARIANT * ); 
unsigned char * __RPC_USER  VARIANT_UserMarshal(  unsigned long *, unsigned char *, VARIANT * ); 
unsigned char * __RPC_USER  VARIANT_UserUnmarshal(unsigned long *, unsigned char *, VARIANT * ); 
void                      __RPC_USER  VARIANT_UserFree(     unsigned long *, VARIANT * ); 

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


