////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "system_xml.h"

//--//

void CLR_RT_XmlState_InName::Setup( CLR_RT_XmlState* st, CLR_UINT8* buffer, CLR_RT_XmlNameType type )
{
    CLR_RT_XmlState_InName* state = &st->InName;
    
    state->stateFn   = &Process;
    state->initFn    = &Init;
    state->cleanUpFn = &CleanUp;

    state->type         = type;
    state->stage        = Xml_ProcessNameStage_Start;
    state->name         = buffer;
    state->prefix       = buffer;
    state->localName    = buffer;
    state->nameLen      = 0;
    state->prefixLen    = 0;
    state->localNameLen = 0;

    switch(type)
    {
    case XmlNameType_Element:
        state->prefixStopMark    = XmlCharType_PrefixStopMark;
        state->localNameStopMark = XmlCharType_LocalNameStopMark;
        break;
        
    case XmlNameType_PITarget:
        state->stage             = Xml_ProcessNameStage_InLocalName;
        state->localNameStopMark = XmlCharType_PITargetStopMark;
        break;

    default: //XmlNameType_Attribute or XmlnameType_XmlDeclAttribute
        state->prefixStopMark    = XmlCharType_AttributePrefixStopMark;
        state->localNameStopMark = XmlCharType_AttributeLocalNameStopMark;
        break;
    }
}

HRESULT CLR_RT_XmlState_InName::Process( CLR_RT_XmlState* st )
{
    TINYCLR_HEADER();
    CLR_RT_XmlState_InName* state = &st->InName;
    
    if(state->stage == Xml_ProcessNameStage_InLocalName)
    {
        goto ProcessLocalName;
    }

    if(FAILED(hr = ProcessNameParts( state, state->nameLen, state->prefixStopMark )))
    {
        state->stage = Xml_ProcessNameStage_InPrefix;
        TINYCLR_LEAVE();
    }

    if(*(state->buffer) == ':')
    {
        state->prefixLen = state->nameLen;
        state->localName = state->buffer + 1;
        state->stage     = Xml_ProcessNameStage_InLocalName;
        state->nameLen++; // for the ':'
        
        if(++(state->buffer) == state->bufferEnd) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);

ProcessLocalName:
        TINYCLR_CHECK_HRESULT(ProcessNameParts( state, state->localNameLen, state->localNameStopMark ));

        state->nameLen += state->localNameLen;
    }
    else
    {
        state->localNameLen = state->nameLen;
    }

    {
        CLR_RT_HeapBlock_XmlReader*    reader    = state->reader;
        CLR_RT_HeapBlock_XmlNameTable* nameTable = reader->GetNameTable();
        
        CLR_RT_HeapBlock_String* name      = NULL;
        CLR_RT_HeapBlock_String* prefix    = NULL;
        CLR_RT_HeapBlock_String* localName = NULL;

        
        CLR_RT_XmlNameType type = state->type;

        TINYCLR_CHECK_HRESULT(nameTable->Add( (LPCSTR)state->name     , state->nameLen     , name      ));
        TINYCLR_CHECK_HRESULT(nameTable->Add( (LPCSTR)state->prefix   , state->prefixLen   , prefix    ));
        TINYCLR_CHECK_HRESULT(nameTable->Add( (LPCSTR)state->localName, state->localNameLen, localName ));

        if(type == XmlNameType_Element || type == XmlNameType_PITarget)
        {
            CLR_RT_HeapBlock_XmlNode* node;

            TINYCLR_CHECK_HRESULT(reader->GetXmlNodes()->Peek( (CLR_RT_HeapBlock*&)node ));

            node->SetName     ( name      );
            node->SetPrefix   ( prefix    );
            node->SetLocalName( localName );

            if(type == XmlNameType_Element)
            {
                CLR_RT_XmlState_InStartElement::Setup( st );
            }
            else
            {
                CLR_RT_XmlState_InProcessingInstruction::Setup( st, state->buffer );
            }
        }
        else // attribute or xml declaration
        {
            CLR_RT_HeapBlock               attributeHB;
            CLR_RT_HeapBlock_XmlAttribute* attribute;
            CLR_INT32                      index;
            
            TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( attributeHB, g_CLR_RT_WellKnownTypes.m_XmlReader_XmlAttribute ));

            attribute = (CLR_RT_HeapBlock_XmlAttribute*)attributeHB.Dereference();

            attribute->SetName     ( name      );
            attribute->SetPrefix   ( prefix    );
            attribute->SetLocalName( localName );

            TINYCLR_CHECK_HRESULT(reader->GetAttributes()->Add( attribute, index ));

            CLR_RT_XmlState_InAttribute::Setup( st );
        }
    }

    TINYCLR_NOCLEANUP();
}

void CLR_RT_XmlState_InName::CleanUp( CLR_RT_XmlState* st )
{
    CLR_RT_XmlState_InName* state = & st->InName;

    state->ShiftBuffer( state->name );
}

void CLR_RT_XmlState_InName::Init( CLR_RT_XmlState* st )
{
    CLR_RT_XmlState_InName* state       = &  st->InName;
    CLR_UINT8*              bufferStart = state->bufferStart;
    CLR_UINT32              prefixLen   = state->prefixLen;

    state->name      = bufferStart;
    state->prefix    = bufferStart;
    state->localName = bufferStart + ((prefixLen > 0) ? prefixLen + 1 : 0);
}

HRESULT CLR_RT_XmlState_InName::ProcessNameParts( CLR_RT_XmlState_InName* state, CLR_UINT32& nameLen, CLR_UINT16 stopsAt )
{
    TINYCLR_HEADER();
    CLR_UINT8* buffer    = state->buffer;
    CLR_UINT8* bufferEnd = state->bufferEnd;
    CLR_UINT32 len       = nameLen;
    
    while(!(c_XmlCharType[ *buffer ] & stopsAt))
    {
        CLR_INT32 count = ProcessNCNameChar( buffer, bufferEnd - buffer, len == 0 );

        if(count > 0)
        {
            len    += count;
            buffer += count;
            if(buffer == bufferEnd) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
        }
        else
        {
            TINYCLR_SET_AND_LEAVE((count == 0) ? XML_E_NEED_MORE_DATA : XML_E_BAD_NAME_CHAR);
        }
    }

    if(len == 0) TINYCLR_SET_AND_LEAVE(XML_E_EMPTY_NAME);

    TINYCLR_CLEANUP();

    nameLen = len;
    state->buffer = buffer;

    TINYCLR_CLEANUP_END();
}

// Returns the number of bytes for the current character
// or 0 if more data is neccessary
// or -1 if the character is not valid
CLR_INT32 CLR_RT_XmlState_InName::ProcessNCNameChar( CLR_UINT8* buffer, CLR_UINT32 count, bool isStartChar )
{
    CLR_UINT8 charTypeNCName = c_XmlCharType[ *buffer ] & XmlCharType_NCNameMask;

    switch(charTypeNCName)
    {
    case XmlCharType_NCNameStartChar1:
        {
            return 1; // valid 1-byte char
        }
 
    case XmlCharType_NCNameChar1:
        {
            return (isStartChar) ? -1 : 1;
        }
    
    case XmlCharType_NCNameChar2:
    case XmlCharType_NCNameChar2Maybe:
        {
            if(count >= 2)
            {
                if(IsNCNameChar2( buffer, isStartChar, charTypeNCName == XmlCharType_NCNameChar2Maybe ))
                {
                    return 2; // valid 2-bytes char
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return 0;
            }
        }
    case XmlCharType_NCNameChar3:
    case XmlCharType_NCNameChar3Maybe:
        {
            if(count >= 3)
            {
                if(IsNCNameChar3( buffer, isStartChar, charTypeNCName == XmlCharType_NCNameChar3Maybe ))
                {
                    return 3; // valid 3-bytes char
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return 0;
            }
        }
    }

    return -1;
}

// check whether this 2-bytes character is a NCNameChar
bool CLR_RT_XmlState_InName::IsNCNameChar2( CLR_UINT8* buffer, bool isStartChar, bool isMaybe )
{
    CLR_UINT8 c0 = buffer[ 0 ];
    CLR_UINT8 c1 = buffer[ 1 ];

    if((c1 & 0xC0) != 0x80) return false;

    if(!isMaybe) return true;

    CLR_UINT16 c = ((c0 & 0x1F) << 6) | (c1 & 0x3F);

    // The rest should not be a "maybe"
    _ASSERTE(c0 == 0xc2 || c0 == 0xc3 || c0 == 0xcc || c0 == 0xcd);

    if(c0 == 0xc2) // 0x0080 - 0x00BF
    {
        // Valid Start Char - None
        // Valid Name Char - 0x00B7
        if(isStartChar) return false;

        return c == 0xb7;
    }
    else if(c0 == 0xc3) // 0x00C0 - 0x00FF
    {
        // Valid Start Char - !0x00D7, !0x00F7
        // Valid Name Char  - Same as Start Char
        return (c != 0xd7) && (c != 0xf7);
    }
    else if(c0 == 0xcc) // 0x0300 - 0x033F
    {
        // Valid Start Char - None
        // Valid Name Char - Everything
        return !isStartChar;
    }
    else //if(c0 == 0xcd) // 0x0340 - 0x037F
    {
        // Valid Start Char - ![0x0340-0x036E], !0x037E
        // Valid Name Char - !0x037E
        if(c == 0x37e) return false;

        return (!isStartChar) || (c < 0x340) || (c > 0x36f);
    }
}

bool CLR_RT_XmlState_InName::IsNCNameChar3( CLR_UINT8* buffer, bool isStartChar, bool isMaybe )
{
    CLR_UINT8 c0 = buffer[ 0 ];
    CLR_UINT8 c1 = buffer[ 1 ];
    CLR_UINT8 c2 = buffer[ 2 ];

    if(((c1 & 0xC0) != 0x80) || ((c2 & 0xC0) != 0x80)) return false;

    if(!isMaybe) return true;

    CLR_UINT16 c = ((c0 & 0x0F) << 12) | ((c1 & 0x3F) << 6) | (c2 & 0x3F);

    if(c0 == 0xe0) // 0x0800 - 0x0FFF
    {
        // 0x0000 - 0x07FF should be expressed using less than 3 bytes, hence illegal
        return c > 0x7ff;
    }
    else if(c0 == 0xe2) // 0x2000 - 0x2FFF
    {
        // Valid Start Char - 0x200C, 0x200D, [0x2070-0x218F], [0x2C00-0x2FEF]
        // Valid Name Char - Valid Start Char + 0x203F, 0x2040
        if(c == 0x200c || c == 0x200d || (c >= 0x2070 && c <= 0x218f) || (c >= 0x2c00 && c <= 0x2fef)) return true;

        return !isStartChar && (c == 0x203f || c == 0x2040);
    }
    else if(c0 == 0xe3) // 0x3000 - 0x3FFF
    {
        // Valid Start Char - !0x3000
        // Valid Name Char - Same as Start Char
        return (c != 0x3000);
    }
    else if(c0 == 0xed) // 0xD000 - 0xDFFF
    {
        // Valid Start Char - [0xD000-0xD7FF]
        // Valid Name Char - Same as Start Char
        return (c >= 0xd000 && c <= 0xd7ff);
    }
    else //if(c0 == 0xef) // 0xF000 - 0xFFFF
    {
        // Valid Start Char - [0xF900-0xFDCF], [0xFDF0-0xFFFD] 
        // Valid Name Char - Same as Start Char
        return (c >= 0xf900 && c <= 0xfdcf) || (c >= 0xfdf0 && c <= 0xfffd);
    }
}

