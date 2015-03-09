////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "system_xml.h"

//--//

void CLR_RT_XmlState_InAttributeValue::Setup( CLR_RT_XmlState* st, CLR_UINT8* buffer, CLR_UINT8 quoteChar )
{
    CLR_RT_XmlState_InAttributeValue* state = &st->InAttributeValue;
    
    state->stateFn   = &Process;
    state->initFn    = &Init;
    state->cleanUpFn = &CleanUp;
    
    state->quoteChar           = quoteChar;
    state->validAttributeChars = (quoteChar == '"') ? XmlCharType_AttributeDQuote : XmlCharType_AttributeSQuote;
    state->value               = buffer;

    state->shiftHelper.Initialize();
}

HRESULT CLR_RT_XmlState_InAttributeValue::Process( CLR_RT_XmlState* st )
{
    TINYCLR_HEADER();
    CLR_RT_XmlState_InAttributeValue* state = &st->InAttributeValue;

    CLR_UINT8* buffer         = state->buffer;
    CLR_UINT8* bufferEnd      = state->bufferEnd;
    CLR_UINT8* value          = state->value;
    CLR_UINT8  validCharTypes = state->validAttributeChars;
    CLR_UINT8  quoteChar      = state->quoteChar;
    bool       isXmlDecl      = (state->type == XmlnameType_XmlDeclAttribute);

    while(true)
    {
        while(c_XmlCharType[ *buffer ] & validCharTypes)
        {
#if 0
            // use this code to decode the %HH hexidecimal character 
            // representations for URI characters as part of the XML spec
            // Extensible Markup Language (XML) 1.0 (Fifth Edition)
            // 4.2.2 External Entities
            bool isHexEncoding = false;

            // For attributes, a URI is encoded with hexidecimal values for 
            // certain characters ':', '/'.  This encoding is of the form
            // %XY where XY is the hexidecimal value for the character.
            if(*buffer == '%' && (&buffer[3] <= bufferEnd))
            {
                CLR_UINT8 val = 0;
                isHexEncoding = true;
               
                for(CLR_INT32 k=1; k<3; k++)
                {
                    val <<= 4;
                    
                    if(buffer[k] >= '0' && buffer[k] <= '9')
                    {
                        val += (buffer[k] - '0');
                    }
                    else if(buffer[k] >= 'a' && buffer[k] <= 'f')
                    {
                        val += (buffer[k] - 'a') + 10;
                    }
                    else if(buffer[k] >= 'A' && buffer[k] <= 'F')
                    {
                        val += (buffer[k] - 'A') + 10;
                    }
                    else
                    {
                        isHexEncoding = false;
                        break;
                    }
                }

                if(isHexEncoding)
                {
                    // Store the converted character value
                    *buffer = val;

                    // collapse the string buffer
                    memmove(&buffer[1], &buffer[3], (CLR_UINT32)bufferEnd - (CLR_UINT32)&buffer[3]);
                    state->bufferEnd -= 2;
                    bufferEnd -= 2;

                    if(buffer == bufferEnd) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
                }
            }
            
            if(!isHexEncoding)
            {
                if(++buffer == bufferEnd) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
            }
#else
            if(++buffer == bufferEnd) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
#endif
        }
        
        if(*buffer == quoteChar)
        {
            break;
        }
        else if(*buffer == '\n' || *buffer == '\t')
        {
            *((LPSTR)buffer) = ' ';
            buffer++;
        }
        else if(*buffer == '\r')
        {
            if(bufferEnd - buffer >= 2)
            {
                if(*(buffer + 1) == '\n')
                {
                    state->shiftHelper.SetNextCarRet( buffer - value, (CLR_UINT8*)buffer );
                    buffer += 2;
                }
                else
                {
                    *((LPSTR)buffer) = ' ';
                    buffer++;
                    continue;
                }
            }
            else
            {
                TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
            }
        }
        else if(*buffer == '&')
        {
            CLR_INT32 count;

            // No character references are allowed in XML Declaration
            if(isXmlDecl) TINYCLR_SET_AND_LEAVE(XML_E_INVALID_XML_DECL);

            count = CLR_RT_XmlCharRef::ParseCharRef( (LPSTR)buffer, bufferEnd - buffer, buffer - state->value, &state->shiftHelper );

            if(count > 0)
            {
                buffer += count;
            }
            else
            {
                TINYCLR_SET_AND_LEAVE((count == 0) ? XML_E_NEED_MORE_DATA : XML_E_BAD_OR_UNSUPPORTED_ENTITY);
            }
        }
        else
        {
            TINYCLR_SET_AND_LEAVE(XML_E_BAD_ATTRIBUTE_CHAR);
        }

        if(buffer == bufferEnd) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
    }
    
    {
        // Shifts all the \r\n and char refs forward
        CLR_RT_HeapBlock_XmlReader*    reader     = state->reader;
        CLR_UINT32                     valueLen   = state->shiftHelper.ShiftBuffer( value, buffer, ' ' );
        CLR_RT_HeapBlock_ArrayList*    attributes = reader->GetAttributes();
        CLR_RT_HeapBlock_XmlAttribute* attribute;
        CLR_RT_HeapBlock_String*       valueString = NULL;

        LPCSTR                         name;

        TINYCLR_CHECK_HRESULT(attributes->GetItem( attributes->GetSize() - 1, (CLR_RT_HeapBlock*&)attribute ));

        name = attribute->GetName()->StringText();

        if(hal_strncmp_s( name, "xmlns", 5 ) == 0 && (name[ 5 ] == 0 || name[ 5 ] == ':')) // namespace declaration
        {
            CLR_RT_HeapBlock_String*  xmlNamespace = state->GetXmlNamespace();
            CLR_RT_HeapBlock_String*  localName    = attribute->GetLocalName();

            bool isLocalNameXml = hal_strncmp_s( localName->StringText(), "xml", 4 ) == 0;

            TINYCLR_CHECK_HRESULT(reader->GetNameTable()->Add( (LPCSTR)value, valueLen, valueString ));

            if((valueString == state->GetXmlnsNamespace()) ||
               (valueString == xmlNamespace && !isLocalNameXml))
            {
                TINYCLR_SET_AND_LEAVE(XML_E_NAMESPACE_DECL_XML_XMLNS);
            }
            
            if(isLocalNameXml)
            {
                if(valueString != xmlNamespace)
                {
                    TINYCLR_SET_AND_LEAVE(XML_E_INVALID_XML_PREFIX_MAPPING);
                }
                // else no-op, since xml is already added during initialization in XmlReader.Create()
            }
            else
            {
                CLR_RT_HeapBlock_XmlNode* xmlNode;
                
                TINYCLR_CHECK_HRESULT(reader->GetNamespaces()->AddNamespace((name[ 5 ] == 0) ? CLR_RT_HeapBlock_String::GetStringEmpty() : localName, valueString ));

                _ASSERTE(reader->GetXmlNodes()->GetSize() > 0);

                reader->GetXmlNodes()->Peek( (CLR_RT_HeapBlock*&)xmlNode );
                xmlNode->IncrementNamespaceCount();
            }
        }
        else
        {
            CLR_RT_HeapBlock ref;

            TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( ref, (LPCSTR)value, valueLen ));

            valueString = ref.DereferenceString();

            if(hal_strncmp_s( attribute->GetPrefix()->StringText(), "xml", 4 ) == 0)
            {
                CLR_RT_HeapBlock_XmlNode* xmlNode;
                
                LPCSTR localName = attribute->GetLocalName()->StringText();

                _ASSERTE(reader->GetXmlNodes()->GetSize() > 0);
                
                reader->GetXmlNodes()->Peek( (CLR_RT_HeapBlock*&)xmlNode );

                if(hal_strncmp_s( localName, "space", 6 ) == 0)
                {
                    LPCSTR    valueStringText = valueString->StringText();
                    CLR_INT32 spaceValue;

                    if(hal_strncmp_s( valueStringText, "preserve", 9 ) == 0)
                    {
                        spaceValue = XmlSpace_Preserve;
                    }
                    else if(hal_strncmp_s( valueStringText, "default", 8 ) == 0)
                    {
                        spaceValue = XmlSpace_Default;
                    }
                    else
                    {
                        TINYCLR_SET_AND_LEAVE(XML_E_INVALID_XML_SPACE);
                    }

                    xmlNode->SetXmlSpace();
                    
                    {
                        // Protect valueString from GC, in case NewObjectFromIndex triggers one
                        CLR_RT_ProtectFromGC gc( ref );
                        CLR_RT_HeapBlock* xmlSpace = g_CLR_RT_ExecutionEngine.ExtractHeapBlocksForClassOrValueTypes( DATATYPE_VALUETYPE, CLR_RT_HeapBlock::HB_Boxed, g_CLR_RT_WellKnownTypes.m_Int32, 2 ); FAULT_ON_NULL(xmlSpace);

                        xmlSpace[ 1 ].SetInteger( spaceValue );
                        
                        TINYCLR_CHECK_HRESULT(reader->GetXmlSpaces()->Push( xmlSpace ));
                    }
                }
                else if(hal_strncmp_s( localName, "lang", 5 ) == 0)
                {
                    xmlNode->SetXmlLang();
                    TINYCLR_CHECK_HRESULT(reader->GetXmlLangs()->Push( valueString ));
                }
            }
        }

        attribute->SetValue( valueString );

        buffer++; // the quote char
    }

    if(isXmlDecl)
    {
        CLR_RT_XmlState_InXmlDeclaration::Setup( st );
    }
    else
    {
        CLR_RT_XmlState_InStartElement::Setup( st );
    }

    if(buffer == bufferEnd)
    {
        TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
    }

    TINYCLR_CLEANUP();

    state->buffer = buffer;

    TINYCLR_CLEANUP_END();
}

void CLR_RT_XmlState_InAttributeValue::CleanUp( CLR_RT_XmlState* st )
{
    CLR_RT_XmlState_InAttributeValue* state = &st->InAttributeValue;

    CLR_UINT8* value = state->value;

    state->ShiftBuffer( value );
    
    state->shiftHelper.SaveRelativePositions( (CLR_UINT8*)value );
}

void CLR_RT_XmlState_InAttributeValue::Init( CLR_RT_XmlState* st )
{
    CLR_RT_XmlState_InAttributeValue* state = &st->InAttributeValue;
    
    state->value = state->bufferStart;
    
    state->shiftHelper.RestoreAbsolutePositions( (CLR_UINT8*)state->value );
}

