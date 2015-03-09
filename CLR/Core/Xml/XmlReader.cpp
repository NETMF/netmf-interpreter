////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "system_xml.h"

//--//

HRESULT CLR_RT_HeapBlock_XmlReader::Read()
{
    TINYCLR_HEADER();
    
    CLR_RT_XmlState* state = GetState();

    state->State.bufferStart = GetBuffer();
    state->State.buffer      = state->State.bufferStart + GetOffset();
    state->State.bufferEnd   = state->State.buffer + GetLength();
    state->State.reader      = this;

    if(state->State.initFn)
    {
        state->State.initFn( state );
    }

    while(true)
    {
        TINYCLR_CHECK_HRESULT(state->State.stateFn( state ));
    }

    TINYCLR_CLEANUP();

    if(hr == XML_E_NEED_MORE_DATA)
    {
        if(state->State.cleanUpFn)
        {
            state->State.cleanUpFn( state );
        }
        else
        {
            state->State.ShiftBuffer( state->State.buffer );
        }
        
        // if after buffer adjustment, there's no room for new data, then we know that
        // we've exceeded one of the system constraints (either the length of name or 
        // attribute value is over buffer size)
        if(state->State.bufferEnd - state->State.bufferStart == XmlBufferSize)
        {
            hr = XML_E_LIMIT_EXCEEDED;
        }
    }

    SetOffset( state->State.buffer    - state->State.bufferStart );
    SetLength( state->State.bufferEnd - state->State.buffer      );

    TINYCLR_CLEANUP_END();
}

HRESULT CLR_RT_HeapBlock_XmlReader::Initialize( CLR_UINT32 settings, CLR_RT_Assembly* systemXmlAssm )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock& ref = ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___state ];
    CLR_RT_HeapBlock_BinaryBlob* blob;
    CLR_RT_XmlState* state;

    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_BinaryBlob::CreateInstance( ref, sizeof(CLR_RT_XmlState), NULL, NULL, 0 ));

    blob  = ref.DereferenceBinaryBlob();
    state = (CLR_RT_XmlState*)blob->GetData();

    // Clear the memory
    CLR_RT_Memory::ZeroFill( state, sizeof(CLR_RT_XmlState) );

    state->State.settings = settings;

    state->State.systemXmlAssm = systemXmlAssm;

    CLR_RT_XmlState_Start::Setup( state );

    TINYCLR_NOCLEANUP();
}

//--//

// Note that prefix and namespaceURI need to be atomized (with nametable) prior to calling AddNamespace
HRESULT CLR_RT_HeapBlock_XmlNamespaceStack::AddNamespace( CLR_RT_HeapBlock_String* prefix, CLR_RT_HeapBlock_String* namespaceURI )
{
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock                    ref;
    CLR_RT_HeapBlock_XmlNamespaceEntry* entry;

    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( ref, g_CLR_RT_WellKnownTypes.m_XmlReader_NamespaceEntry ));

    entry = (CLR_RT_HeapBlock_XmlNamespaceEntry*)ref.Dereference();

    entry->SetPrefix      ( prefix       );
    entry->SetNamespaceURI( namespaceURI );

    TINYCLR_SET_AND_LEAVE(Push( entry ));

    TINYCLR_NOCLEANUP();
}

// Note that prefix needs to be atomized (with nametable) prior to calling LookupNamespace()
CLR_RT_HeapBlock_String* CLR_RT_HeapBlock_XmlNamespaceStack::LookupNamespace( CLR_RT_HeapBlock_String* prefix )
{
    CLR_RT_HeapBlock_Array* array    = GetArray();
    CLR_INT32               size     = GetSize();
    CLR_INT32               capacity = array->m_numOfElements;

    for(int i = capacity - size; i < capacity; i++)
    {
        CLR_RT_HeapBlock_XmlNamespaceEntry* entry = (CLR_RT_HeapBlock_XmlNamespaceEntry*)((CLR_RT_HeapBlock*)array->GetElement( i ))->Dereference();

        // since prefix is atomized, we only need to compare its reference.
        if(entry->GetPrefix() == prefix)
        {
            return entry->GetNamespaceURI();
        }
    }

    return NULL;
}

void CLR_RT_HeapBlock_XmlNamespaceStack::PopScope( CLR_UINT32 nameSpaceCount )
{
    if(nameSpaceCount > 0)
    {
        CLR_RT_HeapBlock_Array* array   = GetArray();
        CLR_INT32               size    = GetSize();
        CLR_INT32               newSize = size - nameSpaceCount;
        CLR_UINT32              index   = array->m_numOfElements - size;

        // We should always have at least 3 entries ("xml", "xmlns", and "" in the namespace stack)
        _ASSERTE(newSize >= 3);

        do
        {
            ((CLR_RT_HeapBlock*)array->GetElement( index++ ))->SetObjectReference( NULL );
        }
        while(--nameSpaceCount != 0);

        SetSize( newSize );
    }
}

//--//

void CLR_RT_HeapBlock_XmlNode::Clear()
{
    CLR_RT_HeapBlock_String* empty = CLR_RT_HeapBlock_String::GetStringEmpty();

    SetName        ( empty );
    SetNamespaceURI( empty );
    SetPrefix      ( empty );
    SetLocalName   ( empty );
    ClearFlags     (       );
}

//--//

CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD_STATIC__Xml == Library_system_xml_native_System_Xml_XmlReader::FIELD_STATIC__Xml);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD_STATIC__Xmlns == Library_system_xml_native_System_Xml_XmlReader::FIELD_STATIC__Xmlns);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD_STATIC__XmlNamespace == Library_system_xml_native_System_Xml_XmlReader::FIELD_STATIC__XmlNamespace);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD_STATIC__XmlnsNamespace == Library_system_xml_native_System_Xml_XmlReader::FIELD_STATIC__XmlnsNamespace);

CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD___buffer == Library_system_xml_native_System_Xml_XmlReader::FIELD___buffer);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD___offset == Library_system_xml_native_System_Xml_XmlReader::FIELD___offset);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD___length == Library_system_xml_native_System_Xml_XmlReader::FIELD___length);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD___xmlNodes == Library_system_xml_native_System_Xml_XmlReader::FIELD___xmlNodes);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD___namespaces == Library_system_xml_native_System_Xml_XmlReader::FIELD___namespaces);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD___xmlSpaces == Library_system_xml_native_System_Xml_XmlReader::FIELD___xmlSpaces);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD___xmlLangs == Library_system_xml_native_System_Xml_XmlReader::FIELD___xmlLangs);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD___nodeType == Library_system_xml_native_System_Xml_XmlReader::FIELD___nodeType);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD___value == Library_system_xml_native_System_Xml_XmlReader::FIELD___value);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD___isEmptyElement == Library_system_xml_native_System_Xml_XmlReader::FIELD___isEmptyElement);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD___attributes == Library_system_xml_native_System_Xml_XmlReader::FIELD___attributes);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD___nameTable == Library_system_xml_native_System_Xml_XmlReader::FIELD___nameTable);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD___state == Library_system_xml_native_System_Xml_XmlReader::FIELD___state);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader__FIELD___isDone == Library_system_xml_native_System_Xml_XmlReader::FIELD___isDone);

CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__Name == Library_system_xml_native_System_Xml_XmlReader_XmlNode::FIELD__Name);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__NamespaceURI == Library_system_xml_native_System_Xml_XmlReader_XmlNode::FIELD__NamespaceURI);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__Prefix == Library_system_xml_native_System_Xml_XmlReader_XmlNode::FIELD__Prefix);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__LocalName == Library_system_xml_native_System_Xml_XmlReader_XmlNode::FIELD__LocalName);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__Flags == Library_system_xml_native_System_Xml_XmlReader_XmlNode::FIELD__Flags);

CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__Name == Library_system_xml_native_System_Xml_XmlReader_XmlAttribute::FIELD__Name);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__NamespaceURI == Library_system_xml_native_System_Xml_XmlReader_XmlAttribute::FIELD__NamespaceURI);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__Prefix == Library_system_xml_native_System_Xml_XmlReader_XmlAttribute::FIELD__Prefix);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__LocalName == Library_system_xml_native_System_Xml_XmlReader_XmlAttribute::FIELD__LocalName);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__Value == Library_system_xml_native_System_Xml_XmlReader_XmlAttribute::FIELD__Value);

CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader_NamespaceEntry__FIELD__Prefix == Library_system_xml_native_System_Xml_XmlReader_NamespaceEntry::FIELD__Prefix);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlReader_NamespaceEntry__FIELD__NamespaceURI == Library_system_xml_native_System_Xml_XmlReader_NamespaceEntry::FIELD__NamespaceURI);

