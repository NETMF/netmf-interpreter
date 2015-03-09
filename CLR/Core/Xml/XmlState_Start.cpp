////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "system_xml.h"

//--//

void CLR_RT_XmlState_Start::Setup( CLR_RT_XmlState* st )
{
    CLR_RT_XmlState_Start* state = &st->Start;
    
    state->stateFn   = &Process;
    state->initFn    = NULL;
    state->cleanUpFn = NULL;
}

HRESULT CLR_RT_XmlState_Start::Process( CLR_RT_XmlState* st )
{
    TINYCLR_HEADER();
    CLR_RT_XmlState_Start* state = &st->Start;

    CLR_UINT8* buffer    = state->buffer;
    CLR_UINT8* bufferEnd = state->bufferEnd;

    CLR_UINT8 c = buffer[ 0 ];

    if(bufferEnd - buffer < 4) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);

    // check for UTF-8 byte order mark
    if(c == 0xEF)
    {
        if(buffer[ 1 ] != 0xBB || buffer[ 2 ] != 0xBF)
        {
            TINYCLR_SET_AND_LEAVE(XML_E_UNKNOWN_ENCODING);
        }

        buffer += 3;

        c = buffer[ 0 ];
    }

    if(c == '<')
    {
        if(buffer[ 1 ] == '?')
        {
            buffer += 2;

            CLR_RT_XmlState_InName::Setup( st, buffer, XmlNameType_PITarget );
        }
        else
        {
            state->docState = XmlDocState_DoneWithXmlDeclaration;
            
            CLR_RT_XmlState_NewTag::Setup( st );
        }
    }
    else if(c_XmlCharType[ c ] & XmlCharType_Space)
    {
        // Once we've seen whitespaces, we can no longer have an xml declaration
        state->docState = XmlDocState_DoneWithXmlDeclaration;

        CLR_RT_XmlState_InValue::SetupWhitespace( st, buffer );
    }
    else // it's not anything that we recognize as a valid start of an XML doc, fail
    {
        TINYCLR_SET_AND_LEAVE(XML_E_UNKNOWN_ENCODING);
    }

    TINYCLR_CLEANUP();

    state->buffer = buffer;

    TINYCLR_CLEANUP_END();
}

