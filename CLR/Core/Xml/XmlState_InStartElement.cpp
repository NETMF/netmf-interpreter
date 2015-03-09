////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "system_xml.h"

//--//

void CLR_RT_XmlState_InStartElement::Setup( CLR_RT_XmlState* st )
{
    CLR_RT_XmlState_InStartElement* state = &st->InStartElement;

    state->stateFn   = &Process;
    state->initFn    = NULL;
    state->cleanUpFn = NULL;
}

HRESULT CLR_RT_XmlState_InStartElement::Process( CLR_RT_XmlState* st )
{
    TINYCLR_HEADER();
    CLR_RT_XmlState_InStartElement* state = &st->InStartElement;
    
    CLR_UINT8* buffer    = state->buffer;
    CLR_UINT8* bufferEnd = state->bufferEnd;
    CLR_UINT8  c;

    while(c_XmlCharType[ *buffer ] & XmlCharType_Space)
    {
        if(++buffer == bufferEnd) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
    }

    c = *buffer;

    if(c == '>')
    {
        buffer++;
        
        TINYCLR_SET_AND_LEAVE(PrepReturn( state, false ));
    }
    else if(c == '/')
    {
        if((bufferEnd - buffer) >= 2)
        {
            if(*(buffer + 1) == '>')
            {
                buffer += 2;

                CLR_RT_HeapBlock_XmlReader* reader = state->reader;

                if(reader->GetXmlNodes()->GetSize() == 1)
                {
                    reader->SetIsDone( true );
                    
                    state->docState = XmlDocState_DoneWithMainDocument;
                }

                TINYCLR_SET_AND_LEAVE(PrepReturn( state, true ));
            }
            else
            {
                TINYCLR_SET_AND_LEAVE(XML_E_UNEXPECTED_TOKEN);
            }
        }
        else
        {
            TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
        }
    }
    else // start of attribute
    {
        CLR_RT_XmlState_InName::Setup( st, buffer, XmlNameType_Attribute );
    }

    TINYCLR_CLEANUP();

    state->buffer = buffer;

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_XmlState_InStartElement::PrepReturn( CLR_RT_XmlState_InStartElement* state, bool isEmptyElement )
{
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock_XmlReader*         reader = state->reader;
    CLR_RT_HeapBlock_XmlNode*           xmlNode;
    CLR_RT_HeapBlock_ArrayList*         attributes = reader->GetAttributes();
    CLR_RT_HeapBlock_XmlNamespaceStack* namespaces = reader->GetNamespaces();
    CLR_RT_HeapBlock_String*            ns;
    CLR_RT_HeapBlock_String*            empty = CLR_RT_HeapBlock_String::GetStringEmpty();
    CLR_RT_HeapBlock_String*            xmlns = state->GetXmlns();

    CLR_UINT32 i = attributes->GetSize();
    CLR_UINT32 j;

    // Go through all the attributes to assign namespaces and find duplicates
    while(i-- != 0)
    {
        CLR_RT_HeapBlock_XmlAttribute* attribute;
        CLR_RT_HeapBlock_String*       prefix;
        
        attributes->GetItem( i, (CLR_RT_HeapBlock*&)attribute );
        
        prefix = attribute->GetPrefix();
        
        if(prefix != empty)
        {
            if(prefix == xmlns)
            {
                ns = state->GetXmlnsNamespace();
            }
            else
            {
                ns = namespaces->LookupNamespace( prefix );
                if(ns == NULL) TINYCLR_SET_AND_LEAVE(XML_E_UNDECLARED_NAMESPACE);
            }
        }
        else
        {
            ns = (attribute->GetName() == xmlns) ? state->GetXmlnsNamespace() : empty;
        }
        
        attribute->SetNamespaceURI( ns );

        j = i;

        while(j-- != 0)
        {
            CLR_RT_HeapBlock_XmlAttribute* attribute2;
            
            attributes->GetItem( j, (CLR_RT_HeapBlock*&)attribute2 );
            
            if((ns == attribute2->GetNamespaceURI()) &&
                (attribute->GetLocalName() == attribute2->GetLocalName()))
            {
                TINYCLR_SET_AND_LEAVE(XML_E_DUP_ATTRIBUTE_NAME);
            }
        }
    }

    TINYCLR_CHECK_HRESULT(reader->GetXmlNodes()->Peek( (CLR_RT_HeapBlock*&)xmlNode ));

    ns = namespaces->LookupNamespace( xmlNode->GetPrefix() );
    if(ns == NULL) TINYCLR_SET_AND_LEAVE(XML_E_UNDECLARED_NAMESPACE);
    xmlNode->SetNamespaceURI( ns );
    
    reader->SetNodeType( XmlNodeType_Element );

    reader->SetIsEmptyElement( isEmptyElement );

    reader->SetValue( empty );
    
    CLR_RT_XmlState_BetweenElements::Setup( (CLR_RT_XmlState*)state, isEmptyElement );
    
    TINYCLR_SET_AND_LEAVE(XML_S_RETURN_TO_MANAGED_CODE);

    TINYCLR_NOCLEANUP();
}

