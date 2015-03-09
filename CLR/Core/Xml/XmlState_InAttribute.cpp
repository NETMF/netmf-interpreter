////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "system_xml.h"

//--//

void CLR_RT_XmlState_InAttribute::Setup( CLR_RT_XmlState* st )
{
    CLR_RT_XmlState_InAttribute* state = &st->InAttribute;
    
    state->stateFn   = &Process;
    state->initFn    = NULL;
    state->cleanUpFn = NULL;
    
    state->seenEqual = false;
}

HRESULT CLR_RT_XmlState_InAttribute::Process( CLR_RT_XmlState* st )
{
    TINYCLR_HEADER();
    CLR_RT_XmlState_InAttribute* state = &st->InAttribute;
    CLR_UINT8* buffer    = state->buffer;
    CLR_UINT8* bufferEnd = state->bufferEnd;

    CLR_UINT32 i = (state->seenEqual) ? 0 : 1;

    // Pass 1: Skip all spaces, check for '='
    // Pass 2: Skip all spaces, done
    do
    {
        while(c_XmlCharType[ *buffer ] & XmlCharType_Space)
        {
            if(++buffer == bufferEnd)
            {
                TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
            }
        }

        if(i == 0) break;

        if(*buffer != '=')
        {
            TINYCLR_SET_AND_LEAVE(XML_E_UNEXPECTED_TOKEN);
        }

        state->seenEqual = true;

        if(++buffer == bufferEnd)
        {
            TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
        }
    }
    while(i-- != 0);

    {
        CLR_UINT8 c = *buffer;
        
        if(c == '"' || c == '\'')
        {
            if(bufferEnd - buffer >= 2)
            {
                buffer++;
                CLR_RT_XmlState_InAttributeValue::Setup( st, buffer, c );
            }
            else
            {
                TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
            }
        }
        else
        {
            TINYCLR_SET_AND_LEAVE(XML_E_UNEXPECTED_TOKEN);
        }

    }
    
    TINYCLR_CLEANUP();

    state->buffer = buffer;

    TINYCLR_CLEANUP_END();
}

