////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "system_xml.h"

//--//

void CLR_RT_XmlState_InProcessingInstruction::Setup( CLR_RT_XmlState* st, CLR_UINT8* buffer )
{
    CLR_RT_XmlState_InProcessingInstruction* state = &st->InProcessingInstruction;
    
    state->stateFn   = &Process;
    state->initFn    = NULL;
    state->cleanUpFn = NULL;
}

HRESULT CLR_RT_XmlState_InProcessingInstruction::Process( CLR_RT_XmlState* st )
{
    TINYCLR_HEADER();
    CLR_RT_XmlState_InProcessingInstruction* state = &st->InProcessingInstruction;

    CLR_RT_HeapBlock_XmlNode* xmlNode;
    LPCSTR                    name;

    TINYCLR_CHECK_HRESULT(state->reader->GetXmlNodes()->Peek( (CLR_RT_HeapBlock*&)xmlNode ));

    name = xmlNode->GetName()->StringText();

    if(hal_stricmp( name, "xml" ) == 0)
    {
        if(hal_strncmp_s( name, "xml", 3 ) == 0)
        {
            if(state->docState == XmlDocState_Start)
            {
                CLR_RT_XmlState_InXmlDeclaration::Setup( st );
            }
            else
            {
                TINYCLR_SET_AND_LEAVE(XML_E_XML_DECL_NOT_FIRST);
            }
        }
        else
        {
            TINYCLR_SET_AND_LEAVE(XML_E_INVALID_PI_NAME);
        }
    }
    else
    {
        if(state->docState == XmlDocState_Start)
        {
            state->docState = XmlDocState_DoneWithXmlDeclaration;
        }
        
        CLR_RT_XmlState_InValue::SetupPI( st, state->buffer );
    }

    TINYCLR_NOCLEANUP();
}

