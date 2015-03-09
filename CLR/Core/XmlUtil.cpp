////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Core.h"

#include <wininet.h>
#include <comutil.h>

#if !defined(PLATFORM_WINCE)
#include <process.h>
#endif

#if defined(PLATFORM_WINCE)
#include <msxml2.h>             //  Required for WinCE XMLDOM stuff...
#endif

#define SAFEBSTR( bstr )  (bstr ? bstr : L"")
#define SAFEASTR( str )   (str  ? str  : "")
#define SAFEWSTR( str )   (str  ? str  : L"")


_COM_SMRT_PTR(IXMLDOMNodeList);
_COM_SMRT_PTR(IXMLDOMElement);
_COM_SMRT_PTR(IXMLDOMProcessingInstruction);
_COM_SMRT_PTR(IXMLDOMAttribute);
_COM_SMRT_PTR(IXMLDOMNamedNodeMap);
_COM_SMRT_PTR(IXMLDOMCDATASection);
_COM_SMRT_PTR(IXMLDOMText);


////////////////////////////////////////////////////////////////////////////////

static HRESULT getStartNode( /*[in ]*/ LPCWSTR         szTag        ,
                             /*[in ]*/ IXMLDOMNode*    pxdnNode     ,
                             /*[out]*/ IXMLDOMNodePtr& xdnNodeStart ,
                             /*[out]*/ bool&           fFound       )
{
    TINYCLR_HEADER();

    //
    // Initialize OUT parameters for 'Not Found' case.
    //
    xdnNodeStart = NULL;
    fFound       = false;

    TINYCLR_PARAMCHECK_BEGIN()
        TINYCLR_PARAMCHECK_NOTNULL(pxdnNode); // No root...
    TINYCLR_PARAMCHECK_END();


    if(szTag)
    {
        _bstr_t tagName(szTag);

        TINYCLR_CHECK_HRESULT(pxdnNode->selectSingleNode( tagName, &xdnNodeStart ));
    }
    else
    {
        xdnNodeStart = pxdnNode;
    }

    if(xdnNodeStart)
    {
        fFound = true;
    }

    TINYCLR_NOCLEANUP();
}

static HRESULT getValueNode( /*[in ]*/ IXMLDOMNode*              pxdnNode ,
                                 /*[out]*/ IXMLDOMNodePtr& xdnValue ,
                                 /*[out]*/ bool&                    fFound   )
{
    TINYCLR_HEADER();

    IXMLDOMNodeListPtr xdnlList;
    IXMLDOMNodePtr     xdnChild;


    //
    // Initialize OUT parameters for 'Not Found' case.
    //
    xdnValue = NULL;
    fFound   = false;

    //
    // Get all the childs of given element.
    //
    TINYCLR_CHECK_HRESULT(pxdnNode->get_childNodes( &xdnlList ));

    //
    // Walk through all the child, searching for a TEXT or CDATA_SECTION.
    //
    for(;SUCCEEDED(hr = xdnlList->nextNode( &xdnChild )) && xdnChild != NULL; xdnChild = NULL)
    {
        DOMNodeType nodeType;

        TINYCLR_CHECK_HRESULT(xdnChild->get_nodeType( &nodeType ));

        if(nodeType == NODE_TEXT          ||
           nodeType == NODE_CDATA_SECTION  )
        {
            //
            // Found...
            //
            xdnValue = xdnChild;
            fFound   = true;
            break;
        }
    }

    TINYCLR_NOCLEANUP();
}

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

CLR_XmlUtil::CLR_XmlUtil( /*[in]*/ const CLR_XmlUtil& xml )
{
    NATIVE_PROFILE_CLR_CORE();
    m_xddDoc  = xml.m_xddDoc;
    m_xdnRoot = xml.m_xdnRoot;
}

CLR_XmlUtil::CLR_XmlUtil( /*[in]*/ IXMLDOMDocument* xddDoc    ,
                             /*[in]*/ LPCWSTR          szRootTag )
{
    IXMLDOMElementPtr xdeElem;
    IXMLDOMNodePtr    xdnRoot;


    if(SUCCEEDED(xddDoc->get_documentElement( &xdeElem )))
    {
        if(SUCCEEDED(xdeElem->QueryInterface( IID_IXMLDOMNode, (void **)&xdnRoot )))
        {
            *this = xdnRoot;
        }
    }

    if(szRootTag)
    {
        bool fLoaded;
        bool fFound;

        (void)LoadPost( szRootTag, fLoaded, &fFound );
    }
}

CLR_XmlUtil::CLR_XmlUtil( /*[in]*/ IXMLDOMNode* xdnRoot   ,
                          /*[in]*/ LPCWSTR      szRootTag )
{
    *this = xdnRoot;


    if(szRootTag)
    {
        bool fLoaded;
        bool fFound;

        (void)LoadPost( szRootTag, fLoaded, &fFound );
    }
}


CLR_XmlUtil::~CLR_XmlUtil()
{
    NATIVE_PROFILE_CLR_CORE();
}


CLR_XmlUtil& CLR_XmlUtil::operator=( /*[in]*/ const CLR_XmlUtil& xml )
{
    NATIVE_PROFILE_CLR_CORE();
    m_xddDoc  = xml.m_xddDoc;
    m_xdnRoot = xml.m_xdnRoot;

    return *this;
}

CLR_XmlUtil& CLR_XmlUtil::operator=( /*[in]*/ IXMLDOMNode* xdnRoot )
{
    NATIVE_PROFILE_CLR_CORE();
    m_xddDoc  = NULL;
    m_xdnRoot = NULL;

    if(xdnRoot)
    {
        if(SUCCEEDED(xdnRoot->get_ownerDocument( &m_xddDoc )))
        {
            m_xdnRoot = xdnRoot;
        }
    }

    return *this;
} 

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

HRESULT CLR_XmlUtil::CreateParser()
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    if(m_xddDoc  != NULL) m_xddDoc .Release();
    if(m_xdnRoot != NULL) m_xdnRoot.Release();

    //
    // Create the DOM object.
    //
    TINYCLR_CHECK_HRESULT(::CoCreateInstance( CLSID_DOMDocument, NULL, CLSCTX_INPROC_SERVER, IID_IXMLDOMDocument, (void**)&m_xddDoc ));

    //
    // Set synchronous operation.
    //
    TINYCLR_CHECK_HRESULT(m_xddDoc->put_async( VARIANT_FALSE ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::New( /*[in]*/ IXMLDOMNode* xdnRoot,
                          /*[in]*/ BOOL         fDeep  )
{
    TINYCLR_HEADER();

    IXMLDOMDocumentPtr xddDoc;
    IXMLDOMNodePtr     xdnNode;
    IXMLDOMNodeListPtr xdnlList;
    IXMLDOMNodePtr     xdnChild;

    TINYCLR_PARAMCHECK_BEGIN()
        TINYCLR_PARAMCHECK_NOTNULL(xdnRoot);
    TINYCLR_PARAMCHECK_END();



    //
    // Create the parser.
    //
    TINYCLR_CHECK_HRESULT(CreateParser());


    //
    // Search the processing elements in the document to clone.
    //
    TINYCLR_CHECK_HRESULT(xdnRoot->get_ownerDocument(                           &xddDoc   ));
    TINYCLR_CHECK_HRESULT(xddDoc ->QueryInterface   ( IID_IXMLDOMNode, (void **)&xdnNode  ));
    TINYCLR_CHECK_HRESULT(xdnNode->get_childNodes   (                           &xdnlList )); xdnNode = NULL;

    for(;SUCCEEDED(hr = xdnlList->nextNode( &xdnChild )) && xdnChild != NULL; xdnChild = NULL)
    {
        DOMNodeType nodeType;

        TINYCLR_CHECK_HRESULT(xdnChild->get_nodeType( &nodeType ));

        //
        // It's a processing element, so clone it.
        //
        if(nodeType == NODE_PROCESSING_INSTRUCTION)
        {
            IXMLDOMNodePtr xdnChildCloned;
            IXMLDOMNodePtr xdnChildNew;

            TINYCLR_CHECK_HRESULT(xdnChild->cloneNode  ( VARIANT_TRUE  , &xdnChildCloned ));
            TINYCLR_CHECK_HRESULT(m_xddDoc->appendChild( xdnChildCloned, &xdnChildNew    ));
        }
    }


    //
    // Clone the node.
    //
    TINYCLR_CHECK_HRESULT(xdnRoot->cloneNode( fDeep ? VARIANT_TRUE : VARIANT_FALSE, &xdnNode ));
    TINYCLR_CHECK_HRESULT(m_xddDoc->appendChild( xdnNode, &m_xdnRoot ));


    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::New( /*[in]*/ LPCWSTR szRootTag  ,
                          /*[in]*/ LPCWSTR szEncoding )
{
    TINYCLR_HEADER();

    TINYCLR_PARAMCHECK_BEGIN()
        TINYCLR_PARAMCHECK_STRING_NOT_EMPTY(szRootTag);
    TINYCLR_PARAMCHECK_END();


    //
    // Create the parser.
    //
    TINYCLR_CHECK_HRESULT(CreateParser());


    //
    // XML header.
    //
    {
        IXMLDOMProcessingInstructionPtr xdpiElem;
        IXMLDOMNodePtr                  xdnNode;
        IXMLDOMNodePtr                  xdnNodeNew;
        _bstr_t                               bstrData( L"version=\"1.0\" encoding=\"" );
        _bstr_t                               bstrPI  ( L"xml"                         );

        bstrData += szEncoding;
        bstrData += "\""      ;

        TINYCLR_CHECK_HRESULT(m_xddDoc->createProcessingInstruction( bstrPI, bstrData, &xdpiElem ));

        TINYCLR_CHECK_HRESULT(xdpiElem->QueryInterface( IID_IXMLDOMNode, (void **)&xdnNode ));
        TINYCLR_CHECK_HRESULT(m_xddDoc->appendChild( xdnNode, &xdnNodeNew ));
    }

    //
    // Create the root node.
    //
    {
        IXMLDOMNodePtr    xdnNode;
        IXMLDOMElementPtr xdeElem;

        TINYCLR_CHECK_HRESULT(m_xddDoc->createElement( _bstr_t( szRootTag ), &xdeElem ));

        TINYCLR_CHECK_HRESULT(xdeElem->QueryInterface( IID_IXMLDOMNode, (void **)&xdnNode ));
        TINYCLR_CHECK_HRESULT(m_xddDoc->appendChild( xdnNode, &m_xdnRoot ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::LoadPost( /*[in ]*/ LPCWSTR szRootTag ,
                               /*[out]*/ bool&   fLoaded   ,
                               /*[out]*/ bool*   fFound    )
{
    TINYCLR_HEADER();

    if(szRootTag)
    {
        _bstr_t              bstrTag( szRootTag );
        IXMLDOMNodePtr xdnNode;

        if(m_xdnRoot)
        {
            TINYCLR_CHECK_HRESULT(m_xdnRoot->selectSingleNode( bstrTag, &xdnNode ));
        }
        else
        {
            TINYCLR_CHECK_HRESULT(m_xddDoc->selectSingleNode( bstrTag, &xdnNode ));
        }

        m_xdnRoot = xdnNode;
    }
    else
    {
        IXMLDOMElementPtr xdeElem;

        TINYCLR_CHECK_HRESULT(m_xddDoc->get_documentElement( &xdeElem ));

        m_xdnRoot.Release();

        if(xdeElem)
        {
            TINYCLR_CHECK_HRESULT(xdeElem->QueryInterface( IID_IXMLDOMNode, (void **)&m_xdnRoot ));
        }
    }

    if(m_xdnRoot)
    {
        if(fFound) *fFound = true;
    }

    fLoaded = true;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::Load( /*[in ]*/ LPCWSTR szFile    ,
                           /*[in ]*/ LPCWSTR szRootTag ,
                           /*[out]*/ bool&   fLoaded   ,
                           /*[out]*/ bool*   fFound    )
{
    TINYCLR_HEADER();

    _variant_t  v( szFile );

    VARIANT_BOOL fSuccess;

    fLoaded = false;
    if(fFound) *fFound = false;


    TINYCLR_CHECK_HRESULT(CreateParser());

    TINYCLR_CHECK_HRESULT(m_xddDoc->load( v, &fSuccess ));
    if(fSuccess == VARIANT_FALSE)
    {
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    TINYCLR_CHECK_HRESULT(LoadPost( szRootTag, fLoaded, fFound ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::LoadAsStream( /*[in ]*/ IUnknown* pStream   ,
                                   /*[in ]*/ LPCWSTR   szRootTag ,
                                   /*[out]*/ bool&     fLoaded   ,
                                   /*[out]*/ bool*     fFound    )
{
    TINYCLR_HEADER();

    _variant_t   v( pStream );
    VARIANT_BOOL fSuccess;

    fLoaded = false;
    if(fFound) *fFound = false;

    //
    // Create the parser.
    //
    TINYCLR_CHECK_HRESULT(CreateParser());

    TINYCLR_CHECK_HRESULT(m_xddDoc->load( v, &fSuccess ));
    if(fSuccess == VARIANT_FALSE)
    {
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    TINYCLR_CHECK_HRESULT(LoadPost( szRootTag, fLoaded, fFound ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::LoadAsString( /*[in ]*/ BSTR    bstrData  ,
                                   /*[in ]*/ LPCWSTR szRootTag ,
                                   /*[out]*/ bool&   fLoaded   ,
                                   /*[out]*/ bool*   fFound    )
{
    TINYCLR_HEADER();

    VARIANT_BOOL fSuccess;


    fLoaded = false;
    if(fFound) *fFound = false;

    //
    // Create the parser.
    //
    TINYCLR_CHECK_HRESULT(CreateParser());


    TINYCLR_CHECK_HRESULT(m_xddDoc->loadXML( bstrData, &fSuccess ));
    if(fSuccess == VARIANT_FALSE)
    {
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    TINYCLR_CHECK_HRESULT(LoadPost( szRootTag, fLoaded, fFound ));

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_XmlUtil::Save( /*[in]*/ LPCWSTR szFile )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_PARAMCHECK_BEGIN()
        TINYCLR_PARAMCHECK_NOTNULL(m_xddDoc); // No document...
        TINYCLR_PARAMCHECK_STRING_NOT_EMPTY(szFile);
    TINYCLR_PARAMCHECK_END();


    TINYCLR_CHECK_HRESULT(m_xddDoc->save( _variant_t( szFile ) ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::SaveAsStream( /*[out]*/ IUnknown* *ppStream )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_PARAMCHECK_BEGIN()
        TINYCLR_PARAMCHECK_NOTNULL(m_xddDoc); // No document...
        TINYCLR_PARAMCHECK_POINTER_AND_SET(ppStream,NULL);
    TINYCLR_PARAMCHECK_END();


    TINYCLR_CHECK_HRESULT(m_xddDoc->QueryInterface( IID_IStream, (void **)ppStream ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::SaveAsString( /*[out]*/ BSTR *pbstrData )
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_PARAMCHECK_BEGIN()
        TINYCLR_PARAMCHECK_NOTNULL(m_xddDoc); // No document...
        TINYCLR_PARAMCHECK_POINTER_AND_SET(pbstrData,NULL);
    TINYCLR_PARAMCHECK_END();


    TINYCLR_CHECK_HRESULT(m_xddDoc->get_xml( pbstrData ));

    TINYCLR_NOCLEANUP();
}

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

HRESULT CLR_XmlUtil::SetVersionAndEncoding( /*[in]*/ LPCWSTR szVersion  ,
                                            /*[in]*/ LPCWSTR szEncoding )
{
    TINYCLR_HEADER();

    IXMLDOMProcessingInstructionPtr xdpiElem;
    IXMLDOMNodePtr                  xdnOldPI;
    IXMLDOMNodePtr                  xdnNewPI;


    TINYCLR_CHECK_HRESULT(m_xddDoc->get_firstChild( &xdnOldPI ));
    if(xdnOldPI)
    {
        _bstr_t bstrTarget(L"xml");
        _bstr_t bstrData(L"version=\"");


        bstrData +=  szVersion         ;
        bstrData +=  L"\" encoding=\"" ;
        bstrData +=  szEncoding        ;
        bstrData +=  "\""              ;


        TINYCLR_CHECK_HRESULT(m_xddDoc->createProcessingInstruction( bstrTarget, bstrData, &xdpiElem ));
        TINYCLR_CHECK_HRESULT(xdpiElem->QueryInterface( IID_IXMLDOMNode, (void **)&xdnNewPI ));

        TINYCLR_CHECK_HRESULT(m_xddDoc->replaceChild( xdnNewPI, xdnOldPI, NULL ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::GetDocument( /*[out]*/ IXMLDOMDocument* *pVal ) const
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_PARAMCHECK_BEGIN()
        TINYCLR_PARAMCHECK_NOTNULL(m_xddDoc); // No document...
        TINYCLR_PARAMCHECK_POINTER_AND_SET(pVal,NULL);
    TINYCLR_PARAMCHECK_END();


    TINYCLR_CHECK_HRESULT(m_xddDoc->QueryInterface( IID_IXMLDOMDocument, (void **)pVal ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::GetRoot( /*[out]*/ IXMLDOMNode* *pVal ) const
{
    NATIVE_PROFILE_CLR_CORE();
    TINYCLR_HEADER();

    TINYCLR_PARAMCHECK_BEGIN()
        TINYCLR_PARAMCHECK_NOTNULL(m_xdnRoot); // No document...
        TINYCLR_PARAMCHECK_POINTER_AND_SET(pVal,NULL);
    TINYCLR_PARAMCHECK_END();


    TINYCLR_CHECK_HRESULT(m_xdnRoot->QueryInterface( IID_IXMLDOMNode, (void **)pVal ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::GetNodes( /*[in ]*/ LPCWSTR           szTag ,
                               /*[out]*/ IXMLDOMNodeList* *pVal  ) const
{
    TINYCLR_HEADER();

    _bstr_t bstrTagName(szTag);

    TINYCLR_PARAMCHECK_BEGIN()
        TINYCLR_PARAMCHECK_NOTNULL(m_xdnRoot); // No root...
        TINYCLR_PARAMCHECK_STRING_NOT_EMPTY(szTag);
        TINYCLR_PARAMCHECK_POINTER_AND_SET(pVal,NULL);
    TINYCLR_PARAMCHECK_END();


    TINYCLR_CHECK_HRESULT(m_xdnRoot->selectNodes( bstrTagName, pVal ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::GetNode( /*[in ]*/ LPCWSTR       szTag ,
                              /*[out]*/ IXMLDOMNode* *pVal  ) const
{
    TINYCLR_HEADER();

    TINYCLR_PARAMCHECK_BEGIN()
        TINYCLR_PARAMCHECK_NOTNULL(m_xdnRoot); // No root...
        TINYCLR_PARAMCHECK_STRING_NOT_EMPTY(szTag);
        TINYCLR_PARAMCHECK_POINTER_AND_SET(pVal,NULL);
    TINYCLR_PARAMCHECK_END();


    TINYCLR_CHECK_HRESULT(m_xdnRoot->selectSingleNode( _bstr_t( szTag ), pVal ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::CreateNode( /*[in ]*/ LPCWSTR       szTag    ,
                                 /*[out]*/ IXMLDOMNode* *pVal     ,
                                 /*[in ]*/ IXMLDOMNode*  pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMElementPtr xdeElem;
    IXMLDOMNodePtr    xdnChild;

    TINYCLR_PARAMCHECK_BEGIN()
        TINYCLR_PARAMCHECK_STRING_NOT_EMPTY(szTag);
        TINYCLR_PARAMCHECK_POINTER_AND_SET(pVal,NULL);
    TINYCLR_PARAMCHECK_END();


    if(m_xdnRoot)
    {
        if(pxdnNode == NULL) { pxdnNode = m_xdnRoot; } // Use root as base of search, if no base node is supplied.

        TINYCLR_CHECK_HRESULT(m_xddDoc->createElement( _bstr_t( szTag ), &xdeElem ));

        TINYCLR_CHECK_HRESULT(xdeElem->QueryInterface( IID_IXMLDOMNode, (void **)&xdnChild ));

        TINYCLR_CHECK_HRESULT(pxdnNode->appendChild( xdnChild, pVal ));
    }
    else
    {
        //
        // No document, so create a new one or attach to the supplied node's document.
        //
        if(pxdnNode)
        {
            TINYCLR_CHECK_HRESULT(New( pxdnNode ));

            TINYCLR_CHECK_HRESULT(CreateNode( szTag, pVal ));
        }
        else
        {
            TINYCLR_CHECK_HRESULT(New( szTag ));

            TINYCLR_CHECK_HRESULT(GetRoot( pVal ));
        }
    }

    TINYCLR_NOCLEANUP();
}

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

HRESULT CLR_XmlUtil::GetAttribute( /*[in ]*/ LPCWSTR            szTag    ,
                                   /*[in ]*/ LPCWSTR            szAttr   ,
                                   /*[out]*/ IXMLDOMAttribute* *pVal     ,
                                   /*[out]*/ bool&              fFound   ,
                                   /*[in ]*/ IXMLDOMNode*       pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMNodePtr xdnNodeStart;
    IXMLDOMNodePtr xdnAttr;

    //
    // Initialize OUT parameters for 'Not Found' case.
    //
    fFound = false;

    TINYCLR_PARAMCHECK_BEGIN()
        TINYCLR_PARAMCHECK_POINTER_AND_SET(pVal,NULL);
    TINYCLR_PARAMCHECK_END();


    if(pxdnNode == NULL) { pxdnNode = m_xdnRoot; } // Use root as base of search, if no base node is supplied.


    TINYCLR_CHECK_HRESULT(getStartNode( szTag, pxdnNode, xdnNodeStart, fFound ));
    if(fFound == false)
    {
        // Node not found...
        fFound = false;
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    if(szAttr)
    {
        _bstr_t                 bstrAttrName(szAttr);
        IXMLDOMNamedNodeMapPtr xdnnmAttrs;

        TINYCLR_CHECK_HRESULT(xdnNodeStart->get_attributes( &xdnnmAttrs ));
        if(xdnnmAttrs == NULL)
        {
            // No attributes...
            fFound = false;
            TINYCLR_SET_AND_LEAVE(S_OK);
        }


        TINYCLR_CHECK_HRESULT(xdnnmAttrs->getNamedItem( bstrAttrName, &xdnAttr ));
        if(hr == S_FALSE)
        {
            // Unknown attribute...
            fFound = false;
            TINYCLR_SET_AND_LEAVE(S_OK);
        }

        TINYCLR_CHECK_HRESULT(xdnAttr->QueryInterface( IID_IXMLDOMAttribute, (void**)pVal ));
    }
    else
    {
        IXMLDOMAttributePtr xdaAttr;

        IUnknown* pUnk = NULL;

        xdnNodeStart->QueryInterface(__uuidof(IXMLDOMAttribute), (void**)&pUnk);

        if(pUnk == NULL)
        {
            // Unknown attribute...
            fFound = false;
            TINYCLR_SET_AND_LEAVE(S_OK);
        }

        xdaAttr = pUnk;

        *pVal = xdaAttr.Detach();
    }

    fFound = true;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::GetAttribute( /*[in ]*/ LPCWSTR       szTag    ,
                                   /*[in ]*/ LPCWSTR       szAttr   ,
                                   /*[out]*/ _bstr_t&     bstrVal  ,
                                   /*[out]*/ bool&         fFound   ,
                                   /*[in ]*/ IXMLDOMNode*  pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMAttributePtr xdaAttr;
    _variant_t               vValue;


    bstrVal = L"";
        
    TINYCLR_CHECK_HRESULT(GetAttribute( szTag, szAttr, &xdaAttr, fFound, pxdnNode ));
    if(fFound == false)
    {
        // Unknown attribute...
        TINYCLR_SET_AND_LEAVE(S_OK);
    }


    TINYCLR_CHECK_HRESULT(xdaAttr->get_value( &vValue ));

    vValue.ChangeType( VT_BSTR );

    bstrVal = vValue.bstrVal;
    fFound  = true;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::GetAttribute( /*[in ]*/ LPCWSTR       szTag    ,
                                   /*[in ]*/ LPCWSTR       szAttr   ,
                                   /*[out]*/ std::wstring& szVal    ,
                                   /*[out]*/ bool&         fFound   ,
                                   /*[in ]*/ IXMLDOMNode*  pxdnNode )
{
    TINYCLR_HEADER();

    _bstr_t bstrVal;


    szVal = L"";


    TINYCLR_CHECK_HRESULT(GetAttribute( szTag, szAttr, bstrVal, fFound, pxdnNode ));

    if(fFound)
    {
        szVal = bstrVal;
    }


    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::GetAttribute( /*[in ]*/ LPCWSTR      szTag    ,
                                   /*[in ]*/ LPCWSTR      szAttr   ,
                                   /*[out]*/ LONG&        lVal     ,
                                   /*[out]*/ bool&        fFound   ,
                                   /*[in ]*/ IXMLDOMNode* pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMAttributePtr xdaAttr;
    _variant_t                vValue;


    lVal = 0;


    TINYCLR_CHECK_HRESULT(GetAttribute( szTag, szAttr, &xdaAttr, fFound, pxdnNode ));
    if(fFound == false)
    {
        // Unknown attribute...
        TINYCLR_SET_AND_LEAVE(S_OK);
    }


    TINYCLR_CHECK_HRESULT(xdaAttr->get_value( &vValue ));

    vValue.ChangeType( VT_I4 );

    lVal   = vValue.lVal;
    fFound = true;


    TINYCLR_NOCLEANUP();
}

/////////////////////////////////////////////////////////////////////////////

HRESULT CLR_XmlUtil::GetValue( /*[in ]*/ LPCWSTR      szTag    ,
                               /*[out]*/ _variant_t& vValue   ,
                               /*[out]*/ bool&        fFound   ,
                               /*[in ]*/ IXMLDOMNode* pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMNodePtr xdnNodeStart;
    IXMLDOMNodePtr xdnChild;


    vValue.Clear();

    if(pxdnNode == NULL) { pxdnNode = m_xdnRoot; } // Use root as base of search, if no base node is supplied.


    TINYCLR_CHECK_HRESULT(getStartNode( szTag, pxdnNode, xdnNodeStart, fFound ));
    if(fFound == false)
    {
        // Node not found...
        TINYCLR_SET_AND_LEAVE(S_OK);
    }


    //
    // Find the node holding the value.
    //
    TINYCLR_CHECK_HRESULT(getValueNode( xdnNodeStart, xdnChild, fFound ));
    if(fFound == false)
    {
        //
        // Not found...
        //
        vValue = L"";
        fFound = true;

        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    //
    // Read the value.
    //
    TINYCLR_CHECK_HRESULT(xdnChild->get_nodeValue( &vValue ));

    fFound = true;

    TINYCLR_NOCLEANUP();
}


HRESULT CLR_XmlUtil::GetValue( /*[in ]*/ LPCWSTR      szTag     ,
                               /*[out]*/ _bstr_t&    bstrValue ,
                               /*[out]*/ bool&        fFound    ,
                               /*[in ]*/ IXMLDOMNode* pxdnNode  )
{
    TINYCLR_HEADER();

    _variant_t vValue;

    TINYCLR_CHECK_HRESULT(GetValue( szTag, vValue, fFound, pxdnNode ));

    if(fFound)
    {
        vValue.ChangeType( VT_BSTR );
        
        bstrValue = vValue.bstrVal;
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::GetValue( /*[in ]*/ LPCWSTR       szTag    ,
                               /*[out]*/ std::wstring& szValue  ,
                               /*[out]*/ bool&         fFound   ,
                               /*[in ]*/ IXMLDOMNode*  pxdnNode )
{
    TINYCLR_HEADER();

    _variant_t vValue;

    TINYCLR_CHECK_HRESULT(GetValue( szTag, vValue, fFound, pxdnNode ));

    if(fFound)
    {
        vValue.ChangeType( VT_BSTR );
        
        szValue = SAFEBSTR( vValue.bstrVal );
    }

    TINYCLR_NOCLEANUP();
}

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

HRESULT CLR_XmlUtil::ModifyAttribute( /*[in ]*/ LPCWSTR         szTag    ,
                                      /*[in ]*/ LPCWSTR         szAttr   ,
                                      /*[in ]*/ const _bstr_t& bstrVal  ,
                                      /*[out]*/ bool&           fFound   ,
                                      /*[in ]*/ IXMLDOMNode*    pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMAttributePtr xdaAttr;
    _variant_t               vValue( bstrVal );


    TINYCLR_CHECK_HRESULT(GetAttribute( szTag, szAttr, &xdaAttr, fFound, pxdnNode ));
    if(fFound == false)
    {
        // Unknown attribute...
        TINYCLR_SET_AND_LEAVE(S_OK);
    }


    TINYCLR_CHECK_HRESULT(xdaAttr->put_value( vValue ));

    fFound = true;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::ModifyAttribute( /*[in ]*/ LPCWSTR             szTag    ,
                                      /*[in ]*/ LPCWSTR             szAttr   ,
                                      /*[in ]*/ const std::wstring& szVal    ,
                                      /*[out]*/ bool&               fFound   ,
                                      /*[in ]*/ IXMLDOMNode*        pxdnNode )
{
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(ModifyAttribute( szTag, szAttr, SAFEWSTR( szVal.c_str() ), fFound, pxdnNode ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::ModifyAttribute( /*[in ]*/ LPCWSTR      szTag    ,
                                      /*[in ]*/ LPCWSTR      szAttr   ,
                                      /*[in ]*/ LPCWSTR      szVal    ,
                                      /*[out]*/ bool&        fFound   ,
                                      /*[in ]*/ IXMLDOMNode* pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMAttributePtr xdaAttr;
    _variant_t               vValue( szVal );


    TINYCLR_CHECK_HRESULT(GetAttribute( szTag, szAttr, &xdaAttr, fFound, pxdnNode ));
    if(fFound == false)
    {
        // Unknown attribute...
        TINYCLR_SET_AND_LEAVE(S_OK);
    }


    TINYCLR_CHECK_HRESULT(xdaAttr->put_value( vValue ));

    fFound = true;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::ModifyAttribute( /*[in ]*/ LPCWSTR      szTag    ,
                                      /*[in ]*/ LPCWSTR      szAttr   ,
                                      /*[in ]*/ LONG         lVal     ,
                                      /*[out]*/ bool&        fFound   ,
                                      /*[in ]*/ IXMLDOMNode* pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMAttributePtr xdaAttr;
    _variant_t               vValue( lVal );


    TINYCLR_CHECK_HRESULT(GetAttribute( szTag, szAttr, &xdaAttr, fFound, pxdnNode ));
    if(fFound == false)
    {
        // Unknown attribute...
        TINYCLR_SET_AND_LEAVE(S_OK);
    }


    TINYCLR_CHECK_HRESULT(xdaAttr->put_value( vValue ));

    fFound = true;

    TINYCLR_NOCLEANUP();
}

/////////////////////////////////////////////////////////////////////////////

HRESULT CLR_XmlUtil::ModifyValue( /*[in ]*/ LPCWSTR            szTag    ,
                                  /*[in ]*/ const _variant_t& vValue   ,
                                  /*[out]*/ bool&              fFound   ,
                                  /*[in ]*/ IXMLDOMNode*       pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMNodePtr xdnNodeStart;
    IXMLDOMNodePtr xdnChild;


    if(pxdnNode == NULL) { pxdnNode = m_xdnRoot; } // Use root as base of search, if no base node is supplied.


    TINYCLR_CHECK_HRESULT(getStartNode( szTag, pxdnNode, xdnNodeStart, fFound ));
    if(fFound == false)
    {
        // Node not found...
        TINYCLR_SET_AND_LEAVE(S_OK);
    }


    //
    // Find the node holding the value.
    //
    TINYCLR_CHECK_HRESULT(getValueNode( xdnNodeStart, xdnChild, fFound ));
    if(fFound == false)
    {
        //
        // Not found...
        //
        TINYCLR_SET_AND_LEAVE(S_OK);
    }


    //
    // Modify the value.
    //
    TINYCLR_CHECK_HRESULT(xdnChild->put_nodeValue( vValue ));

    fFound = true;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::ModifyValue( /*[in ]*/ LPCWSTR         szTag     ,
                                  /*[in ]*/ const _bstr_t& bstrValue ,
                                  /*[out]*/ bool&           fFound    ,
                                  /*[out]*/ IXMLDOMNode*    pxdnNode  )
{

    TINYCLR_HEADER();

    _variant_t vValue = bstrValue;

    TINYCLR_SET_AND_LEAVE(ModifyValue( szTag, vValue, fFound, pxdnNode ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::ModifyValue( /*[in ]*/ LPCWSTR             szTag    ,
                                  /*[in ]*/ const std::wstring& szValue  ,
                                  /*[out]*/ bool&               fFound   ,
                                  /*[out]*/ IXMLDOMNode*        pxdnNode )
{
    TINYCLR_HEADER();

    _variant_t vValue = szValue.c_str();

    TINYCLR_SET_AND_LEAVE(ModifyValue( szTag, vValue, fFound, pxdnNode ));

    TINYCLR_NOCLEANUP();
}

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////

HRESULT CLR_XmlUtil::PutAttribute( /*[in ]*/ LPCWSTR            szTag    ,
                                   /*[in ]*/ LPCWSTR            szAttr   ,
                                   /*[out]*/ IXMLDOMAttribute* *pVal     ,
                                   /*[out]*/ bool&              fFound   ,
                                   /*[in ]*/ IXMLDOMNode*       pxdnNode )
{
    TINYCLR_HEADER();

    _bstr_t                     bstrAttrName = szAttr;
    IXMLDOMNodePtr         xdnNodeStart;
    IXMLDOMNamedNodeMapPtr xdnnmAttrs;
    IXMLDOMNodePtr         xdnAttr;

    TINYCLR_PARAMCHECK_BEGIN()
        TINYCLR_PARAMCHECK_STRING_NOT_EMPTY(szAttr);
        TINYCLR_PARAMCHECK_POINTER_AND_SET(pVal,NULL);
    TINYCLR_PARAMCHECK_END();


    if(pxdnNode == NULL) { pxdnNode = m_xdnRoot; } // Use root as base of search, if no base node is supplied.


    TINYCLR_CHECK_HRESULT(getStartNode( szTag, pxdnNode, xdnNodeStart, fFound ));
    if(fFound == false)
    {
        // Node not found...
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    TINYCLR_CHECK_HRESULT(xdnNodeStart->get_attributes( &xdnnmAttrs ));
    if(xdnnmAttrs == NULL)
    {
        // No attributes...
        fFound = false;
        TINYCLR_SET_AND_LEAVE(S_OK);
    }


    TINYCLR_CHECK_HRESULT(xdnnmAttrs->getNamedItem( bstrAttrName, &xdnAttr ));
    if(hr == S_FALSE)
    {
        TINYCLR_CHECK_HRESULT(m_xddDoc->createAttribute( bstrAttrName, pVal ));

        TINYCLR_CHECK_HRESULT(xdnnmAttrs->setNamedItem( *pVal, NULL ));
    }
    else
    {
        TINYCLR_CHECK_HRESULT(xdnAttr->QueryInterface( IID_IXMLDOMAttribute, (void**)pVal ));
    }

    fFound = true;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::PutAttribute( /*[in ]*/ LPCWSTR         szTag    ,
                                   /*[in ]*/ LPCWSTR         szAttr   ,
                                   /*[in ]*/ const _bstr_t& bstrVal  ,
                                   /*[out]*/ bool&           fFound   ,
                                   /*[in ]*/ IXMLDOMNode*    pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMAttributePtr xdaAttr;
    _variant_t               vValue( bstrVal );


    TINYCLR_CHECK_HRESULT(PutAttribute( szTag, szAttr, &xdaAttr, fFound, pxdnNode ));
    if(fFound == true)
    {
        TINYCLR_CHECK_HRESULT(xdaAttr->put_value( vValue ));
    }

    fFound = true;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::PutAttribute( /*[in ]*/ LPCWSTR             szTag    ,
                                   /*[in ]*/ LPCWSTR             szAttr   ,
                                   /*[in ]*/ const std::wstring& szVal    ,
                                   /*[out]*/ bool&               fFound   ,
                                   /*[in ]*/ IXMLDOMNode*        pxdnNode )
{
    TINYCLR_HEADER();

    TINYCLR_SET_AND_LEAVE(PutAttribute( szTag, szAttr, SAFEWSTR( szVal.c_str() ), fFound, pxdnNode ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::PutAttribute( /*[in ]*/ LPCWSTR      szTag    ,
                                   /*[in ]*/ LPCWSTR      szAttr   ,
                                   /*[in ]*/ LPCWSTR      szVal    ,
                                   /*[out]*/ bool&        fFound   ,
                                   /*[in ]*/ IXMLDOMNode* pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMAttributePtr xdaAttr;
    _variant_t               vValue( szVal );


    TINYCLR_CHECK_HRESULT(PutAttribute( szTag, szAttr, &xdaAttr, fFound, pxdnNode ));
    if(fFound == true)
    {
        TINYCLR_CHECK_HRESULT(xdaAttr->put_value( vValue ));
    }

    fFound = true;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::PutAttribute( /*[in ]*/ LPCWSTR      szTag    ,
                                   /*[in ]*/ LPCWSTR      szAttr   ,
                                   /*[in ]*/ LONG         lVal     ,
                                   /*[out]*/ bool&        fFound   ,
                                   /*[in ]*/ IXMLDOMNode* pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMAttributePtr xdaAttr;
    _variant_t               vValue( lVal );


    TINYCLR_CHECK_HRESULT(PutAttribute( szTag, szAttr, &xdaAttr, fFound, pxdnNode ));
    if(fFound == true)
    {
        TINYCLR_CHECK_HRESULT(xdaAttr->put_value( vValue ));
    }

    fFound = true;

    TINYCLR_NOCLEANUP();
}

/////////////////////////////////////////////////////////////////////////////

HRESULT CLR_XmlUtil::PutValue( /*[in ]*/ LPCWSTR            szTag    ,
                               /*[in ]*/ const _variant_t& vValue   ,
                               /*[out]*/ bool&              fFound   ,
                               /*[out]*/ IXMLDOMNode*       pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMNodePtr xdnNodeStart;
    IXMLDOMNodePtr xdnChild;


    if(pxdnNode == NULL) { pxdnNode = m_xdnRoot; } // Use root as base of search, if no base node is supplied.


    TINYCLR_CHECK_HRESULT(getStartNode( szTag, pxdnNode, xdnNodeStart, fFound ));
    if(fFound == false)
    {
        if(szTag == NULL)
        {
            // Node not found...
            TINYCLR_SET_AND_LEAVE(S_OK);
        }

        //
        // Tag supplied, let's try to create a new node.
        //
        TINYCLR_CHECK_HRESULT(CreateNode( szTag, &xdnNodeStart, pxdnNode ));
    }


    //
    // Find the node holding the value.
    //
    TINYCLR_CHECK_HRESULT(getValueNode( xdnNodeStart, xdnChild, fFound ));
    if(fFound == false)
    {
        if(vValue.vt == VT_ARRAY)
        {
            IXMLDOMCDATASectionPtr xdcData;

            TINYCLR_CHECK_HRESULT(m_xddDoc->createCDATASection( NULL, &xdcData ));

            TINYCLR_CHECK_HRESULT(xdcData->QueryInterface( IID_IXMLDOMNode, (void **)&xdnChild ));
        }
        else
        {
            IXMLDOMTextPtr xdtData;

            TINYCLR_CHECK_HRESULT(m_xddDoc->createTextNode( NULL, &xdtData ));

            TINYCLR_CHECK_HRESULT(xdtData->QueryInterface( IID_IXMLDOMNode, (void **)&xdnChild ));
        }

        TINYCLR_CHECK_HRESULT(xdnNodeStart->appendChild( xdnChild, NULL ));
    }


    //
    // Modify the value.
    //
    TINYCLR_CHECK_HRESULT(xdnChild->put_nodeValue( vValue ));

    fFound = true;

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::PutValue( /*[in ]*/ LPCWSTR         szTag     ,
                               /*[in ]*/ const _bstr_t& bstrValue ,
                               /*[out]*/ bool&           fFound    ,
                               /*[out]*/ IXMLDOMNode*    pxdnNode  )
{
    TINYCLR_HEADER();

    _variant_t vValue = bstrValue;

    TINYCLR_SET_AND_LEAVE(PutValue( szTag, vValue, fFound, pxdnNode ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::PutValue( /*[in ]*/ LPCWSTR             szTag    ,
                               /*[in ]*/ const std::wstring& szValue  ,
                               /*[out]*/ bool&               fFound   ,
                               /*[out]*/ IXMLDOMNode*        pxdnNode )
{
    TINYCLR_HEADER();

    _variant_t vValue = szValue.c_str();

    TINYCLR_SET_AND_LEAVE(PutValue( szTag, vValue, fFound, pxdnNode ));

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::PutValue( /*[in ]*/ LPCWSTR      szTag    ,
                               /*[in ]*/ LPCWSTR      szValue  ,
                               /*[out]*/ bool&        fFound   ,
                               /*[out]*/ IXMLDOMNode* pxdnNode )
{
    TINYCLR_HEADER();

    _variant_t vValue = szValue;

    TINYCLR_SET_AND_LEAVE(PutValue( szTag, vValue, fFound, pxdnNode ));

    TINYCLR_NOCLEANUP();
}

////////////////////////////////////////

HRESULT CLR_XmlUtil::RemoveAttribute( /*[in]*/ LPCWSTR      szTag    ,
                                      /*[in]*/ LPCWSTR      szAttr   ,
                                      /*[in]*/ IXMLDOMNode* pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMNodePtr xdnNodeStart;
    bool                 fFound;


    if(pxdnNode == NULL) { pxdnNode = m_xdnRoot; } // Use root as base of search, if no base node is supplied.


    TINYCLR_CHECK_HRESULT(getStartNode( szTag, pxdnNode, xdnNodeStart, fFound ));
    if(fFound)
    {
        IXMLDOMNamedNodeMapPtr xdnnmAttrs;

        TINYCLR_CHECK_HRESULT(xdnNodeStart->get_attributes( &xdnnmAttrs ));
        if(xdnnmAttrs)
        {
            IXMLDOMNodePtr xdnAttr;

            (void)xdnnmAttrs->removeNamedItem( _bstr_t( szAttr ), &xdnAttr );
        }
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::RemoveValue( /*[in]*/ LPCWSTR      szTag    ,
                                  /*[in]*/ IXMLDOMNode* pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMNodePtr xdnNodeStart;
    bool                 fFound;


    if(pxdnNode == NULL) { pxdnNode = m_xdnRoot; } // Use root as base of search, if no base node is supplied.


    TINYCLR_CHECK_HRESULT(getStartNode( szTag, pxdnNode, xdnNodeStart, fFound ));
    if(fFound)
    {
        ; // This method is currently not implemented
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_XmlUtil::RemoveNode( /*[in]*/ LPCWSTR      szTag    ,
                                 /*[in]*/ IXMLDOMNode* pxdnNode )
{
    TINYCLR_HEADER();

    IXMLDOMNodePtr xdnNodeStart;
    bool                 fFound;


    if(pxdnNode == NULL) { pxdnNode = m_xdnRoot; } // Use root as base of search, if no base node is supplied.


    TINYCLR_CHECK_HRESULT(getStartNode( szTag, pxdnNode, xdnNodeStart, fFound ));
    if(fFound)
    {
        IXMLDOMNodePtr xdnNodeParent;

        TINYCLR_CHECK_HRESULT(xdnNodeStart->get_parentNode( &xdnNodeParent ));
        if(xdnNodeParent)
        {
            IXMLDOMNodePtr xdnNodeChild;

            TINYCLR_CHECK_HRESULT(xdnNodeParent->removeChild( xdnNodeStart, &xdnNodeChild ));
        }
    }

    TINYCLR_NOCLEANUP();
}
