////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "system_xml.h"

//--//

void CLR_RT_XmlState_InValue::Setup( CLR_RT_XmlState* st, CLR_UINT8* buffer, bool processOnly )
{
    CLR_RT_XmlState_InValue* state = &st->InValue;
    
    state->stateFn   = &Process;
    state->initFn    = &Init;
    state->cleanUpFn = &CleanUp;

    if(!processOnly)
    {
        state->value = buffer;

        state->shiftHelper.Initialize();
        state->valueChunks.Initialize();
    }
}

HRESULT CLR_RT_XmlState_InValue::Process( CLR_RT_XmlState* st )
{
    TINYCLR_HEADER();
    CLR_RT_XmlState_InValue* state = &st->InValue;
    
    CLR_UINT8* buffer    = state->buffer;
    CLR_UINT8* bufferEnd = state->bufferEnd;
    CLR_UINT8* value     = state->value;

    bool processOnly          = state->processOnly;
    CLR_UINT16 normalCharType = state->normalCharType;
    CLR_UINT8 endValueChar    = state->endValueChar;
    
    while(true)
    {
        while(c_XmlCharType[ *buffer ] & normalCharType)
        {
            if(++buffer == bufferEnd) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
        }

        if(*buffer == '\r')
        {
            if(bufferEnd - buffer >= 2)
            {
                if(buffer[ 1 ] == '\n')
                {
                    if(!processOnly)
                    {
                        state->shiftHelper.SetNextCarRet( buffer - value, (CLR_UINT8*)buffer );
                    }
                    buffer += 2;
                }
                else
                {
                    *((LPSTR)buffer) = '\n';
                    buffer++;
                    continue;
                }
            }
            else
            {
                TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
            }
        }
        else if(*buffer == endValueChar)
        {
            bool isDone = true;
            
            hr = state->endFn( st, buffer, isDone );

            if(isDone) TINYCLR_LEAVE();
        }
        else if(*buffer == '&')
        {
            CLR_INT32 count = CLR_RT_XmlCharRef::ParseCharRef( (LPSTR)buffer, bufferEnd - buffer, buffer - state->value, &state->shiftHelper );

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
            bool isDone = true;
            
            hr = state->othersFn( st, buffer, isDone );

            if(isDone) TINYCLR_LEAVE();
        }

        if(buffer == bufferEnd) TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
    }

    TINYCLR_CLEANUP();

    state->buffer = buffer;

    TINYCLR_CLEANUP_END();
}

void CLR_RT_XmlState_InValue::CleanUp( CLR_RT_XmlState* st )
{
    CLR_RT_XmlState_InValue* state = &st->InValue;

    CLR_UINT8* buffer = state->buffer;

    if(!state->processOnly)
    {
        CLR_UINT8* value    = state->value;
        CLR_UINT32 valueLen = state->shiftHelper.ShiftBuffer( value, buffer, '\n' );

        state->valueChunks.Add( value, valueLen );
    }

    state->ShiftBuffer( buffer );
}

void CLR_RT_XmlState_InValue::Init( CLR_RT_XmlState* st )
{
    CLR_RT_XmlState_InValue* state = &st->InValue;

    if(!state->processOnly)
    {
        state->value = state->bufferStart;
        
        // reset the shift helper
        state->shiftHelper.Initialize();
    }
}

//--//

HRESULT CLR_RT_XmlState_InValue::PrepReturn( CLR_RT_XmlState* st, CLR_UINT8*& buffer, CLR_UINT32 count, CLR_RT_XmlNodeType nodeType )
{
    TINYCLR_HEADER();

    CLR_RT_XmlState_InValue* state = &st->InValue;

    if(!state->processOnly)
    {
        CLR_RT_HeapBlock_XmlReader* reader = state->reader;
        CLR_UINT8* value = state->value;
        
        CLR_UINT32 valueLen = state->shiftHelper.ShiftBuffer( value, buffer, '\n' );
        
        TINYCLR_CHECK_HRESULT(state->valueChunks.SetValueAndUninitialize( reader, value, valueLen ));
        
        reader->SetNodeType( nodeType );
        
        buffer += count;
        
        TINYCLR_SET_AND_LEAVE(XML_S_RETURN_TO_MANAGED_CODE);
    }
    else
    {
        buffer += count;
    }

    TINYCLR_NOCLEANUP();
}

//--//

void CLR_RT_XmlState_InValue::SetupComment( CLR_RT_XmlState* st, CLR_UINT8* buffer )
{
    CLR_RT_XmlState_InValue* state = &st->InValue;

    bool processOnly = (state->settings & XmlSettings_IgnoreComments) != 0;

    state->processOnly    = processOnly;
    state->endFn          = &ProcessEndComment;
    state->othersFn       = &ProcessOthersInvalidChar;
    state->normalCharType = XmlCharType_Comment;
    state->endValueChar   = '-';

    Setup( st, buffer, processOnly );
}

HRESULT CLR_RT_XmlState_InValue::ProcessEndComment( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone )
{
    TINYCLR_HEADER();

    _ASSERTE(isDone == true);
    
    if(st->State.bufferEnd - buffer >= 3)
    {
        if(buffer[ 1 ] == '-')
        {
            if(buffer[ 2 ] == '>') // end of comment
            {
                hr = PrepReturn( st, buffer, 3, XmlNodeType_Comment );
                
                CLR_RT_XmlState_BetweenElements::Setup( st, true );

                if(st->State.docState == XmlDocState_DoneWithMainDocument)
                {
                    st->State.reader->SetIsDone( true );
                }
                
                TINYCLR_LEAVE();
            }
            else // "--" are not allow in the comment
            {
                TINYCLR_SET_AND_LEAVE(XML_E_INVALID_COMMENT_CHARS);
            }
        }
        else // it's just a single '-', we'll continue processing
        {
            isDone = false;
            buffer++;
        }
    }
    else
    {
        TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
    }

    TINYCLR_NOCLEANUP();
}

//--//

void CLR_RT_XmlState_InValue::SetupPI( CLR_RT_XmlState* st, CLR_UINT8* buffer )
{
    CLR_RT_XmlState_InValue* state = &st->InValue;

    bool processOnly = (state->settings & XmlSettings_IgnoreProcessingInstructions) != 0;

    state->processOnly    = processOnly;
    state->endFn          = &ProcessEndPI;
    state->othersFn       = &ProcessOthersInvalidChar;
    state->normalCharType = XmlCharType_PI;
    state->endValueChar   = '?';

    Setup( st, buffer, processOnly );
}

HRESULT CLR_RT_XmlState_InValue::ProcessEndPI( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone )
{
    TINYCLR_HEADER();

    _ASSERTE(isDone == true);
    
    if(st->State.bufferEnd - buffer >= 2)
    {
        if(buffer[ 1 ] == '>')// end of PI
        {
            hr = PrepReturn( st, buffer, 2, XmlNodeType_ProcessingInstruction );
            
            CLR_RT_XmlState_BetweenElements::Setup( st, true );
            
            if(st->State.docState == XmlDocState_DoneWithMainDocument)
            {
                st->State.reader->SetIsDone( true );
            }
                            
            TINYCLR_LEAVE();
        }
        else // it's just a single '?', we'll continue processing
        {
            isDone = false;
            buffer++;
        }
    }
    else
    {
        TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
    }

    TINYCLR_NOCLEANUP();
}

//--//

void CLR_RT_XmlState_InValue::SetupCDATA( CLR_RT_XmlState* st, CLR_UINT8* buffer )
{
    CLR_RT_XmlState_InValue* state = &st->InValue;

    state->processOnly    = false;
    state->endFn          = &ProcessEndCDATA;
    state->othersFn       = &ProcessOthersInvalidChar;
    state->normalCharType = XmlCharType_CDATA;
    state->endValueChar   = ']';

    Setup( st, buffer, false );
}

HRESULT CLR_RT_XmlState_InValue::ProcessEndCDATA( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone )
{
    TINYCLR_HEADER();

    _ASSERTE(isDone == true);

    if(st->State.bufferEnd - buffer >= 3)
    {
        if(buffer[ 1 ] == ']' && buffer[ 2 ] == '>') // end of CDATA
        {
            hr = PrepReturn( st, buffer, 3, XmlNodeType_CDATA );
            
            CLR_RT_XmlState_BetweenElements::Setup( st, true );
            
            TINYCLR_LEAVE();
        }
        else // it's not "]]>", we'll continue processing
        {
            isDone = false;
            buffer++;
        }
    }
    else
    {
        TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
    }

    TINYCLR_NOCLEANUP();
}

//--//

void CLR_RT_XmlState_InValue::SetupText( CLR_RT_XmlState* st, CLR_UINT8* buffer, bool fromWhitespace )
{
    CLR_RT_XmlState_InValue* state = &st->InValue;

    state->endFn          = &ProcessEndText;
    state->othersFn       = &ProcessOthersText;
    state->normalCharType = XmlCharType_Text;

    if(!fromWhitespace)
    {
        state->processOnly  = false;
        state->endValueChar = '<';
        Setup( st, buffer, false );
    }
}

HRESULT CLR_RT_XmlState_InValue::ProcessEndText( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone )
{
    _ASSERTE(isDone == true);

    HRESULT hr = PrepReturn( st, buffer, 0, XmlNodeType_Text );
    
    CLR_RT_XmlState_NewTag::Setup( st );

    return hr;
}

HRESULT CLR_RT_XmlState_InValue::ProcessOthersText( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone )
{
    TINYCLR_HEADER();

    _ASSERTE(isDone == true);
    
    if(*buffer == ']')
    {
        CLR_UINT32 count = st->State.bufferEnd - buffer;

        if(count >= 3 && buffer[ 1 ] == ']' && buffer[ 2 ] == '>')
        {
            TINYCLR_SET_AND_LEAVE(XML_E_CDATA_END_IN_TEXT);
        }
        else if(count == 1 || (count == 2 && buffer[ 1 ] == ']'))
        {
            TINYCLR_SET_AND_LEAVE(XML_E_NEED_MORE_DATA);
        }
        else
        {
            isDone = false;
            buffer++;
        }
    }
    else
    {
        TINYCLR_SET_AND_LEAVE(XML_E_INVALID_CHARACTER);
    }
    
    TINYCLR_NOCLEANUP();
}

//--//

void CLR_RT_XmlState_InValue::SetupWhitespace( CLR_RT_XmlState* st, CLR_UINT8* buffer )
{
    CLR_RT_XmlState_InValue* state = &st->InValue;

    state->processOnly    = false;
    state->endFn          = &ProcessEndWhitespace;
    state->othersFn       = &ProcessOthersWhitespace;
    state->normalCharType = XmlCharType_Whitespace;
    state->endValueChar   = '<';

    Setup( st, buffer, false );
}

HRESULT CLR_RT_XmlState_InValue::ProcessEndWhitespace( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone )
{
    TINYCLR_HEADER();

    _ASSERTE(isDone == true);

    CLR_RT_XmlState_InValue* state = &st->InValue;

    CLR_RT_HeapBlock*       xmlSpace;
    CLR_RT_XmlSpace         xmlSpaceValue;
    
    TINYCLR_CHECK_HRESULT(state->reader->GetXmlSpaces()->Peek( xmlSpace ));

    // note that xmlSpace is boxed    
    _ASSERTE( xmlSpace->DataType() == DATATYPE_VALUETYPE );
    xmlSpaceValue = (CLR_RT_XmlSpace)xmlSpace[ 1 ].NumericByRef().s4;

    if(xmlSpaceValue == XmlSpace_Preserve || !(state->settings & XmlSettings_IgnoreWhitespace))
    {
        TINYCLR_SET_AND_LEAVE(PrepReturn( st, buffer, 0, (xmlSpaceValue == XmlSpace_Preserve) ? XmlNodeType_SignificantWhitespace : XmlNodeType_Whitespace ));
    }

    TINYCLR_CLEANUP();

    // regardless of result, we'll set up the next state. note that it's not possible to get XML_E_NEED_MORE_DATA here
    CLR_RT_XmlState_NewTag::Setup( st );

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_XmlState_InValue::ProcessOthersWhitespace( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone )
{
    TINYCLR_HEADER();

    _ASSERTE(isDone == true);

    if(st->State.docState != XmlDocState_MainDocument)
    {
        TINYCLR_SET_AND_LEAVE(XML_E_INVALID_ROOT_DATA);
    }

    SetupText( st, buffer, true );

    TINYCLR_NOCLEANUP();
}

//--//

HRESULT CLR_RT_XmlState_InValue::ProcessOthersInvalidChar( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone )
{
    _ASSERTE(isDone == true);
    
    return XML_E_INVALID_CHARACTER;
}

