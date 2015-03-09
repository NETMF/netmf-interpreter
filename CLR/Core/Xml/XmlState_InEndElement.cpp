////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "system_xml.h"

//--//

void CLR_RT_XmlState_InEndElement::Setup( CLR_RT_XmlState* st, CLR_UINT8* buffer )
{
    CLR_RT_XmlState_InEndElement* state = &st->InEndElement;
    
    state->stateFn   = &Process;
    state->initFn    = &Init;
    state->cleanUpFn = &CleanUp;

    state->name               = buffer;
    state->nameLen            = 0;
    state->doneWithName       = false;
}

HRESULT CLR_RT_XmlState_InEndElement::Process( CLR_RT_XmlState* st )
{
    TINYCLR_HEADER();
    CLR_RT_XmlState_InEndElement* state = &st->InEndElement;

    CLR_UINT8* buffer    = state->buffer;
    CLR_UINT8* bufferEnd = state->bufferEnd;
    CLR_UINT8* name      = state->name;
    CLR_UINT32 nameLen   = state->nameLen;

    if(!state->doneWithName)
    {
        while(!(c_XmlCharType[ *buffer ] & (XmlCharType_Space | XmlCharType_GT)))
        {
            if(++buffer == bufferEnd) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
        }

        nameLen = buffer - name;
    }

    state->doneWithName = true;

    while(c_XmlCharType[ *buffer ] & XmlCharType_Space)
    {
        if(++buffer == bufferEnd) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
    }

    if(*buffer != '>')
    {
        TINYCLR_SET_AND_LEAVE(XML_E_UNEXPECTED_TOKEN);
    }

    buffer++;

    {
        CLR_RT_HeapBlock_XmlReader* reader   = state->reader;
        CLR_RT_HeapBlock_Stack*     xmlNodes = reader->GetXmlNodes();
        CLR_RT_HeapBlock_XmlNode*   xmlNode;
        LPCSTR                      startElementName;

        if(FAILED(xmlNodes->Pop ( (CLR_RT_HeapBlock*&)xmlNode )) ||
           FAILED(xmlNodes->Peek( (CLR_RT_HeapBlock*&)xmlNode )))
        {
            TINYCLR_SET_AND_LEAVE(XML_E_UNEXPECTED_END_TAG);
        }

        startElementName = xmlNode->GetName()->StringText();

        if((hal_strncmp_s( startElementName, (LPCSTR)name, nameLen ) != 0) || (startElementName[ nameLen ] != '\0'))
        {
            TINYCLR_SET_AND_LEAVE(XML_E_TAG_MISMATCH);
        }

        if(xmlNodes->GetSize() == 1)
        {
            reader->SetIsDone( true );

            state->docState = XmlDocState_DoneWithMainDocument;
        }

        CLR_RT_XmlState_BetweenElements::Setup( st, true );

        reader->SetNodeType( XmlNodeType_EndElement );
        
        reader->SetValue( CLR_RT_HeapBlock_String::GetStringEmpty() );

        TINYCLR_SET_AND_LEAVE(XML_S_RETURN_TO_MANAGED_CODE);
    }

    TINYCLR_CLEANUP();

    state->buffer = buffer;
    state->nameLen = nameLen;

    TINYCLR_CLEANUP_END();
}

void CLR_RT_XmlState_InEndElement::CleanUp( CLR_RT_XmlState* st )
{
    CLR_RT_XmlState_InEndElement* state = &st->InEndElement;

    state->ShiftBuffer( state->name );
}

void CLR_RT_XmlState_InEndElement::Init( CLR_RT_XmlState* st )
{
    CLR_RT_XmlState_InEndElement* state = &st->InEndElement;
    
    state->name = state->bufferStart;
}

