////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "system_xml.h"

//--//

void CLR_RT_XmlState_InXmlDeclaration::Setup( CLR_RT_XmlState* st )
{
    CLR_RT_XmlState_InXmlDeclaration* state = &st->InXmlDeclaration;

    state->stateFn   = &Process;
    state->initFn    = NULL;
    state->cleanUpFn = NULL;
}

HRESULT CLR_RT_XmlState_InXmlDeclaration::Process( CLR_RT_XmlState* st )
{
    TINYCLR_HEADER();
    CLR_RT_XmlState_InXmlDeclaration* state = &st->InXmlDeclaration;
    
    CLR_UINT8* buffer    = state->buffer;
    CLR_UINT8* bufferEnd = state->bufferEnd;

    while(c_XmlCharType[ *buffer ] & XmlCharType_Space)
    {
        if(++buffer == bufferEnd) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
    }

    if(*buffer == '?')
    {
        if((bufferEnd - buffer) >= 2)
        {
            if(buffer[ 1 ] == '>')
            {
                buffer += 2;
                
                CLR_RT_HeapBlock_XmlReader* reader         = state->reader;
                CLR_RT_HeapBlock_ArrayList* attributes     = reader->GetAttributes();
                CLR_UINT32                  attributeCount = attributes->GetSize();
                CLR_RT_HeapBlock_String*    empty          = CLR_RT_HeapBlock_String::GetStringEmpty();
                
                if(attributeCount == 0)
                {
                    TINYCLR_SET_AND_LEAVE(XML_E_INVALID_XML_DECL);
                }
                
                // Verify version, encoding & standalone attributes
                CLR_UINT32 xmlDeclState = 0;   // <?xml (0) version='1.0' (1) encoding='__' (2) standalone='__' (3) ?>
                CLR_UINT32 i            = 0;
                
                do
                {
                    CLR_RT_HeapBlock_XmlAttribute* attribute;
                    LPCSTR                         name;
                    LPCSTR                         value;
                
                    attributes->GetItem( i++, (CLR_RT_HeapBlock*&)attribute );
                    
                    name  = attribute->GetName()->StringText();
                    value = attribute->GetValue()->StringText();

                    if((xmlDeclState == 0) && (hal_strncmp_s( name, "version", 8 ) == 0) && 
                        (hal_strncmp_s( value, "1.0", 4 ) == 0))
                    {
                        xmlDeclState = 1;
                    }
                    else if((xmlDeclState == 1) && (hal_strncmp_s( name, "encoding", 9 ) == 0) &&
                             (hal_stricmp( value, "utf-8" ) == 0))
                    {
                        xmlDeclState = 2;
                    }
                    else if((xmlDeclState == 1 || xmlDeclState == 2) && (hal_strncmp_s( name, "standalone", 11 ) == 0) &&
                            (hal_strncmp_s( value, "no", 3 ) == 0 || hal_strncmp_s( value, "yes", 4 ) == 0))
                    {
                        xmlDeclState = 3;
                    }
                    else
                    {
                        TINYCLR_SET_AND_LEAVE(XML_E_INVALID_XML_DECL);
                    }

                    attribute->SetNamespaceURI( empty );
                    
                } while(--attributeCount != 0);
                
                state->docState = XmlDocState_DoneWithXmlDeclaration;
                
                reader->SetNodeType( XmlNodeType_XmlDeclaration );
                
                CLR_RT_XmlState_BetweenElements::Setup( (CLR_RT_XmlState*)state, true );
                
                TINYCLR_SET_AND_LEAVE(XML_S_RETURN_TO_MANAGED_CODE);
            }
            else
            {
                TINYCLR_SET_AND_LEAVE(XML_E_INVALID_XML_DECL);
            }
        }
        else
        {
            TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
        }
    }
    else // start of "attribute"
    {
        CLR_RT_XmlState_InName::Setup( st, buffer, XmlnameType_XmlDeclAttribute );
    }

    TINYCLR_CLEANUP();

    state->buffer = buffer;

    TINYCLR_CLEANUP_END();
}

