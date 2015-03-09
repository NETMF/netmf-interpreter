////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "system_xml.h"

//--//

void CLR_RT_XmlState_NewTag::Setup( CLR_RT_XmlState* st )
{
    CLR_RT_XmlState_NewTag* state = &st->NewTag;
    
    state->stateFn   = &Process;
    state->initFn    = NULL;
    state->cleanUpFn = NULL;
}

HRESULT CLR_RT_XmlState_NewTag::Process( CLR_RT_XmlState* st )
{
    TINYCLR_HEADER();
    CLR_RT_XmlState_NewTag* state = &st->NewTag;

    CLR_UINT8* buffer                  = state->buffer;
    CLR_UINT8* bufferEnd               = state->bufferEnd;
    CLR_RT_HeapBlock_XmlReader* reader = state->reader;

    CLR_UINT32 count = bufferEnd - buffer;

    _ASSERTE(buffer[ 0 ] == '<');
    
    reader->SetIsDone( false );

    // in the minimal case, there will be at least 4 bytes left in the stream
    // (<a/> or </a>), so we'll check for at least 4 bytes left and save ourselves
    // for the trouble to compare count in every case
    if(count < 4) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);

    switch(buffer[ 1 ])
    {
    case '/':

        buffer += 2;

        CLR_RT_XmlState_InEndElement::Setup( st, buffer );
        
        break;
    case '!':
        if(buffer[ 2 ] == '-' && buffer[ 3 ] == '-')
        {
            buffer += 4;
            CLR_RT_XmlState_InValue::SetupComment( st, buffer );
        }
        else if(buffer[ 2 ] == '[')
        {
            if(count < 9) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);

            if(hal_strncmp_s( (LPCSTR)(buffer + 3), "CDATA[", 6 ) == 0)
            {
                if(state->docState != XmlDocState_MainDocument)
                {
                    TINYCLR_SET_AND_LEAVE(XML_E_INVALID_ROOT_DATA);
                }
                
                buffer += 9;
                CLR_RT_XmlState_InValue::SetupCDATA( st, buffer );
            }
            else
            {
                TINYCLR_SET_AND_LEAVE(XML_E_UNEXPECTED_TOKEN);
            }
        }
        else
        {
            TINYCLR_SET_AND_LEAVE(XML_E_DTD_IS_PROHIBITED);
        }

        // we only need to check this for comment and CDATA, because in all
        // other cases, we're guarantee to have at least 2 bytes left in the 
        // buffer, because of the count < 4 check above.
        if(buffer == bufferEnd) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
        
        break;
    case '?':
        
        buffer += 2;
        CLR_RT_XmlState_InName::Setup( st, buffer, XmlNameType_PITarget );
        
        break;
    default:
        if(state->docState == XmlDocState_DoneWithXmlDeclaration)
        {
            state->docState = XmlDocState_MainDocument;
        }
        else if(state->docState == XmlDocState_DoneWithMainDocument)
        {
            TINYCLR_SET_AND_LEAVE(XML_E_MULTIPLE_ROOTS);
        }
        
        buffer++;
        CLR_RT_XmlState_InName::Setup( st, buffer, XmlNameType_Element );
        
        break;
    }

    TINYCLR_CLEANUP();

    state->buffer = buffer;

    TINYCLR_CLEANUP_END();
}

