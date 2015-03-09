////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "system_xml.h"

//--//

void CLR_RT_XmlState_BetweenElements::Setup( CLR_RT_XmlState* st, bool fromEndElement )
{
    CLR_RT_XmlState_BetweenElements* state = &st->BetweenElements;
    
    state->stateFn   = &Process;
    state->initFn    = NULL;
    state->cleanUpFn = NULL;
    
    state->fromEndElement = fromEndElement;
}

HRESULT CLR_RT_XmlState_BetweenElements::Process( CLR_RT_XmlState* st )
{
    TINYCLR_HEADER();
    CLR_RT_XmlState_BetweenElements* state = &st->BetweenElements;

    CLR_RT_HeapBlock_XmlReader* reader   = state->reader;
    CLR_RT_HeapBlock_Stack*     xmlNodes = reader->GetXmlNodes();
    CLR_RT_HeapBlock_XmlNode*   xmlNode;

    CLR_UINT8* buffer = state->buffer;

    if(state->fromEndElement)
    {
        CLR_UINT32 namespaceCount;
        bool hasXmlSpace;
        bool hasXmlLang;
        
        CLR_RT_HeapBlock* tmp;

        _ASSERTE(xmlNodes->GetSize() > 0);
        _ASSERTE(reader->GetXmlSpaces()->GetSize() > 0);
        _ASSERTE(reader->GetXmlLangs()->GetSize() > 0);
        
        xmlNodes->Peek( (CLR_RT_HeapBlock*&)xmlNode );

        xmlNode->GetFlags( namespaceCount, hasXmlSpace, hasXmlLang );

        reader->GetNamespaces()->PopScope( namespaceCount );

        if(hasXmlSpace)
        {
            reader->GetXmlSpaces()->Pop( tmp );
        }

        if(hasXmlLang)
        {
            reader->GetXmlLangs()->Pop( tmp );
        }

        reader->SetIsEmptyElement( false );
    }
    else
    {
        CLR_RT_HeapBlock xmlNodeHB;

        TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( xmlNodeHB, g_CLR_RT_WellKnownTypes.m_XmlReader_XmlNode ));

        xmlNode = (CLR_RT_HeapBlock_XmlNode*)xmlNodeHB.Dereference();

        TINYCLR_CHECK_HRESULT(xmlNodes->Push( xmlNode ));
    }

    xmlNode->Clear();

    TINYCLR_CHECK_HRESULT(reader->GetAttributes()->Clear());

    if(*buffer == '<')
    {
        CLR_RT_XmlState_NewTag::Setup( st );
    }
    else if(c_XmlCharType[ *buffer ] & XmlCharType_Space)
    {
        CLR_RT_XmlState_InValue::SetupWhitespace( st, buffer );
    }
    else
    {
        if(state->docState != XmlDocState_MainDocument)
        {
            TINYCLR_SET_AND_LEAVE(XML_E_INVALID_ROOT_DATA);
        }
        
        CLR_RT_XmlState_InValue::SetupText( st, buffer, false );
    }

    TINYCLR_NOCLEANUP();
}

