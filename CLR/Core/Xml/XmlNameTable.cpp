////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "system_xml.h"

//--//

// May Trigger GC if get is false, str, if not NULL, needs to be rooted prior to calling Add()
HRESULT CLR_RT_HeapBlock_XmlNameTable::Add( LPCSTR key, CLR_UINT32 length, CLR_RT_HeapBlock_String*& str, bool get )
{
    TINYCLR_HEADER();

    CLR_RT_HeapBlock_XmlNameTable_Entry* entry;
    CLR_RT_HeapBlock_String*             match;

    CLR_INT32  hashCode;
    CLR_UINT32 i;

    if(length == 0)
    {
        // if length is 0, return String.Empty
        str = CLR_RT_HeapBlock_String::GetStringEmpty();
        TINYCLR_SET_AND_LEAVE(S_OK);
    }

    // Calculating the hash code
    hashCode = length + GetRandomizer();

    for(i = 0; i < length; i++)
    {
        hashCode += (hashCode << 7) ^ key[ i ];
    }

    hashCode -= hashCode >> 17;
    hashCode -= hashCode >> 11;
    hashCode -= hashCode >> 5;

    // Retrieve the entry (or entries) that hash to that bucket
    entry = (CLR_RT_HeapBlock_XmlNameTable_Entry*)(((CLR_RT_HeapBlock*)GetEntries()->GetElement( hashCode & GetMask() ))->Dereference());

    // Go through all the entries in the singly linked list to make sure there's no match
    while(entry != NULL)
    {
        if(entry->GetHashCode() == hashCode)
        {
            match = entry->GetStr();
            if((hal_strncmp_s( match->StringText(), key, length ) == 0) && (match->StringText()[ length ] == '\0'))
            {
                // if we find a match, we return the matched string
                str = match;
                TINYCLR_SET_AND_LEAVE(S_OK);
            }
        }

        entry = entry->GetNext();
    }

    if(get)
    {
        // we're only getting, so return null if no string is found
        str = NULL;
        TINYCLR_SET_AND_LEAVE(S_OK);
    }
    else
    {
        // we'll re-use the String object if we were given one, if not, we'll create it here
        if(str == NULL)
        {
            CLR_RT_HeapBlock strHB;
            
            TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_String::CreateInstance( strHB, key, length ));
            
            str = strHB.DereferenceString();

            // Attach the new string to the managed handle to prevent it from being GC when we allocate the Entry object in AddEntry()
            SetTmp( str );
        }

        TINYCLR_SET_AND_LEAVE(AddEntry( str, hashCode ));
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_XmlNameTable::AddEntry( CLR_RT_HeapBlock_String*& str, CLR_INT32 hashCode )
{
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock                     newEntryHB;
    CLR_RT_HeapBlock_XmlNameTable_Entry* newEntry;
    CLR_RT_HeapBlock*                    entryHB;

    CLR_INT32                            count;  

    // create a new instance of the Entry object
    TINYCLR_CHECK_HRESULT(g_CLR_RT_ExecutionEngine.NewObjectFromIndex( newEntryHB, g_CLR_RT_WellKnownTypes.m_XmlNameTable_Entry ));
    
    newEntry = (CLR_RT_HeapBlock_XmlNameTable_Entry*)newEntryHB.Dereference();
    
    // attach it to the front of the bucket
    entryHB = (CLR_RT_HeapBlock*)GetEntries()->GetElement( GetMask() & hashCode );

    newEntry->SetStr( str );
    newEntry->SetHashCode( hashCode );
    newEntry->SetNext( (CLR_RT_HeapBlock_XmlNameTable_Entry*)entryHB->Dereference() );
    
    entryHB->SetObjectReference( newEntry );

    count = GetCount() + 1;
    
    SetCount( count );

    // if we reach the threshold, we'll grow the buckets
    if(count == GetMask())
    {
        Grow();
    }

    TINYCLR_NOCLEANUP();
}

HRESULT CLR_RT_HeapBlock_XmlNameTable::Grow()
{
    TINYCLR_HEADER();
    
    CLR_RT_HeapBlock_Array*              oldEntries;
    CLR_RT_HeapBlock                     newEntriesHB;
    CLR_RT_HeapBlock_Array*              newEntries;
    CLR_RT_HeapBlock_XmlNameTable_Entry* entry;
    CLR_RT_HeapBlock_XmlNameTable_Entry* next;
    CLR_RT_HeapBlock*                    newEntryHB;

    CLR_UINT32                           i;
    CLR_INT32                            newIndex;
    CLR_INT32                            newMask;

    newMask = GetMask() * 2 + 1;

    // allocate a new instance of Entry array
    TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance( newEntriesHB, newMask + 1, g_CLR_RT_WellKnownTypes.m_XmlNameTable_Entry ));

    newEntries = newEntriesHB.DereferenceArray();
    oldEntries = GetEntries();

    // Go through the old buckets, and resort them
    for(i = 0; i < oldEntries->m_numOfElements; i++)
    {
        entry = (CLR_RT_HeapBlock_XmlNameTable_Entry*)((CLR_RT_HeapBlock*)oldEntries->GetElement( i ))->Dereference();
    
        while(entry != NULL)
        {
            newIndex = entry->GetHashCode() & newMask;
            next = entry->GetNext();
    
            newEntryHB = (CLR_RT_HeapBlock*)newEntries->GetElement( newIndex );
    
            entry->SetNext( (CLR_RT_HeapBlock_XmlNameTable_Entry*)newEntryHB->Dereference() );
    
            newEntryHB->SetObjectReference( entry );
    
            entry = next;
        }
    }

    SetEntries( newEntries );
    SetMask   ( newMask    );

    TINYCLR_NOCLEANUP();
}

//--//

CT_ASSERT(Library_system_xml_native_System_Xml_XmlNameTable__FIELD___entries == Library_system_xml_native_System_Xml_XmlNameTable::FIELD___entries);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlNameTable__FIELD___count == Library_system_xml_native_System_Xml_XmlNameTable::FIELD___count);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlNameTable__FIELD___mask == Library_system_xml_native_System_Xml_XmlNameTable::FIELD___mask);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlNameTable__FIELD___hashCodeRandomizer == Library_system_xml_native_System_Xml_XmlNameTable::FIELD___hashCodeRandomizer);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlNameTable__FIELD___tmp == Library_system_xml_native_System_Xml_XmlNameTable::FIELD___tmp);

CT_ASSERT(Library_system_xml_native_System_Xml_XmlNameTable_Entry__FIELD__Str == Library_system_xml_native_System_Xml_XmlNameTable_Entry::FIELD__Str);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlNameTable_Entry__FIELD__HashCode == Library_system_xml_native_System_Xml_XmlNameTable_Entry::FIELD__HashCode);
CT_ASSERT(Library_system_xml_native_System_Xml_XmlNameTable_Entry__FIELD__Next == Library_system_xml_native_System_Xml_XmlNameTable_Entry::FIELD__Next);

