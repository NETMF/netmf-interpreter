////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef _SYSTEM_XML_NATIVE_H_
#define _SYSTEM_XML_NATIVE_H_

#include <TinyCLR_Interop.h>
#include <TinyCLR_Runtime.h>

struct Library_system_xml_native_System_Xml_XmlNameTable
{
    static const int FIELD___entries            = 1;
    static const int FIELD___count              = 2;
    static const int FIELD___mask               = 3;
    static const int FIELD___hashCodeRandomizer = 4;
    static const int FIELD___tmp                = 5;

    TINYCLR_NATIVE_DECLARE(Get___STRING__STRING);
    TINYCLR_NATIVE_DECLARE(Add___STRING__STRING);

    //--//

};

struct Library_system_xml_native_System_Xml_XmlNameTable_Entry
{
    static const int FIELD__Str      = 1;
    static const int FIELD__HashCode = 2;
    static const int FIELD__Next     = 3;


    //--//

};

struct Library_system_xml_native_System_Xml_XmlReader
{
    static const int FIELD_STATIC__Xml            = 0;
    static const int FIELD_STATIC__Xmlns          = 1;
    static const int FIELD_STATIC__XmlNamespace   = 2;
    static const int FIELD_STATIC__XmlnsNamespace = 3;

    static const int FIELD___buffer           = 1;
    static const int FIELD___offset           = 2;
    static const int FIELD___length           = 3;
    static const int FIELD___xmlNodes         = 4;
    static const int FIELD___namespaces       = 5;
    static const int FIELD___xmlSpaces        = 6;
    static const int FIELD___xmlLangs         = 7;
    static const int FIELD___nodeType         = 8;
    static const int FIELD___value            = 9;
    static const int FIELD___isEmptyElement   = 10;
    static const int FIELD___attributes       = 11;
    static const int FIELD___nameTable        = 12;
    static const int FIELD___state            = 13;
    static const int FIELD___isDone           = 14;
    static const int FIELD___currentAttribute = 15;
    static const int FIELD___stream           = 16;
    static const int FIELD___readState        = 17;

    TINYCLR_NATIVE_DECLARE(LookupNamespace___STRING__STRING);
    TINYCLR_NATIVE_DECLARE(Initialize___VOID__U4);
    TINYCLR_NATIVE_DECLARE(ReadInternal___I4__U4);
    TINYCLR_NATIVE_DECLARE(StringRefEquals___STATIC__BOOLEAN__STRING__STRING);


    //--//

};


struct Library_system_xml_native_System_Xml_XmlReaderSettings
{
    static const int FIELD__NameTable                    = 1;
    static const int FIELD__IgnoreWhitespace             = 2;
    static const int FIELD__IgnoreProcessingInstructions = 3;
    static const int FIELD__IgnoreComments               = 4;


    //--//

};

struct Library_system_xml_native_System_Xml_XmlReader_NamespaceEntry
{
    static const int FIELD__Prefix       = 1;
    static const int FIELD__NamespaceURI = 2;


    //--//

};

struct Library_system_xml_native_System_Xml_XmlReader_XmlAttribute
{
    static const int FIELD__Name         = 1;
    static const int FIELD__NamespaceURI = 2;
    static const int FIELD__Prefix       = 3;
    static const int FIELD__LocalName    = 4;
    static const int FIELD__Value        = 5;


    //--//

};

struct Library_system_xml_native_System_Xml_XmlReader_XmlNode
{
    static const int FIELD__Name         = 1;
    static const int FIELD__NamespaceURI = 2;
    static const int FIELD__Prefix       = 3;
    static const int FIELD__LocalName    = 4;
    static const int FIELD__Flags        = 5;


    //--//

};



extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_System_Xml;

#endif  //_SYSTEM_XML_NATIVE_H_
