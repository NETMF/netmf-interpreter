////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_XML_H_
#define _TINYCLR_XML_H_

#include <TinyCLR_Types.h>
#include <TinyCLR_Runtime.h>

//--//

// Xml Buffer Length -- Keep in sync with XmlReader.BufferSize in System.Xml.dll (XmlReader.cs)
#define XmlBufferSize 1024

// Xml Error codes -- Keep in sync with XmlExceptionErrorCode in System.Xml.dll (XmlException.cs)

#define XML_S_RETURN_TO_MANAGED_CODE              MAKE_HRESULT( SEVERITY_ERROR, 0x5000, 0x0000 )

#define XML_E_NEED_MORE_DATA                      MAKE_HRESULT( SEVERITY_ERROR, 0x5100, 0x0000 )

#define XML_E_ERROR                               MAKE_HRESULT( SEVERITY_ERROR, 0x5200, 0x0000 )
#define XML_E_UNEXPECTED_EOF                      MAKE_HRESULT( SEVERITY_ERROR, 0x5300, 0x0000 )
#define XML_E_BAD_NAME_CHAR                       MAKE_HRESULT( SEVERITY_ERROR, 0x5400, 0x0000 )
#define XML_E_UNKNOWN_ENCODING                    MAKE_HRESULT( SEVERITY_ERROR, 0x5500, 0x0000 )
#define XML_E_UNEXPECTED_TOKEN                    MAKE_HRESULT( SEVERITY_ERROR, 0x5600, 0x0000 )
#define XML_E_TAG_MISMATCH                        MAKE_HRESULT( SEVERITY_ERROR, 0x5700, 0x0000 )
#define XML_E_UNEXPECTED_END_TAG                  MAKE_HRESULT( SEVERITY_ERROR, 0x5800, 0x0000 )
#define XML_E_BAD_ATTRIBUTE_CHAR                  MAKE_HRESULT( SEVERITY_ERROR, 0x5900, 0x0000 )
#define XML_E_MULTIPLE_ROOTS                      MAKE_HRESULT( SEVERITY_ERROR, 0x5A00, 0x0000 )
#define XML_E_INVALID_ROOT_DATA                   MAKE_HRESULT( SEVERITY_ERROR, 0x5B00, 0x0000 )
#define XML_E_XML_DECL_NOT_FIRST                  MAKE_HRESULT( SEVERITY_ERROR, 0x5C00, 0x0000 )
#define XML_E_INVALID_XML_DECL                    MAKE_HRESULT( SEVERITY_ERROR, 0x5D00, 0x0000 )
#define XML_E_INVALID_XML_SPACE                   MAKE_HRESULT( SEVERITY_ERROR, 0x5E00, 0x0000 )
#define XML_E_DUP_ATTRIBUTE_NAME                  MAKE_HRESULT( SEVERITY_ERROR, 0x5F00, 0x0000 )
#define XML_E_INVALID_CHARACTER                   MAKE_HRESULT( SEVERITY_ERROR, 0x6000, 0x0000 )
#define XML_E_CDATA_END_IN_TEXT                   MAKE_HRESULT( SEVERITY_ERROR, 0x6100, 0x0000 )
#define XML_E_INVALID_COMMENT_CHARS               MAKE_HRESULT( SEVERITY_ERROR, 0x6200, 0x0000 )
#define XML_E_LIMIT_EXCEEDED                      MAKE_HRESULT( SEVERITY_ERROR, 0x6300, 0x0000 )
#define XML_E_BAD_OR_UNSUPPORTED_ENTITY           MAKE_HRESULT( SEVERITY_ERROR, 0x6400, 0x0000 )
#define XML_E_UNDECLARED_NAMESPACE                MAKE_HRESULT( SEVERITY_ERROR, 0x6500, 0x0000 )
#define XML_E_INVALID_XML_PREFIX_MAPPING          MAKE_HRESULT( SEVERITY_ERROR, 0x6600, 0x0000 )
#define XML_E_NAMESPACE_DECL_XML_XMLNS            MAKE_HRESULT( SEVERITY_ERROR, 0x6700, 0x0000 )
#define XML_E_INVALID_PI_NAME                     MAKE_HRESULT( SEVERITY_ERROR, 0x6800, 0x0000 )
#define XML_E_DTD_IS_PROHIBITED                   MAKE_HRESULT( SEVERITY_ERROR, 0x6900, 0x0000 )
#define XML_E_EMPTY_NAME                          MAKE_HRESULT( SEVERITY_ERROR, 0x6A00, 0x0000 )
#define XML_E_INVALID_NODE_TYPE                   MAKE_HRESULT( SEVERITY_ERROR, 0x6B00, 0x0000 )
#define XML_E_ELEMENT_NOT_FOUND                   MAKE_HRESULT( SEVERITY_ERROR, 0x6C00, 0x0000 )

#define IS_XML_ERROR_CODE(hr) ((hr) >= XML_S_RETURN_TO_MANAGED_CODE)

//--//

// Please keep in-sync with XmlNodeType enum in System.Xml.dll (XmlNodeType.cs)
enum CLR_RT_XmlNodeType
{
    XmlNodeType_None                  = 0,
    XmlNodeType_Element               = 1,
    XmlNodeType_Attribute             = 2,
    XmlNodeType_Text                  = 3,
    XmlNodeType_CDATA                 = 4,
    XmlNodeType_ProcessingInstruction = 5,
    XmlNodeType_Comment               = 6,
    XmlNodeType_Whitespace            = 7,
    XmlNodeType_SignificantWhitespace = 8,
    XmlNodeType_EndElement            = 9,
    XmlNodeType_XmlDeclaration        = 10,
};

// Please keep in-sync with XmlSpace enum in System.Xml.dll (XmlSpace.cs)
enum CLR_RT_XmlSpace
{
    XmlSpace_None     = 0,
    XmlSpace_Default  = 1,
    XmlSpace_Preserve = 2,
};

// Please keep in-sync with consts defined in XmlReaderSettings in System.Xml.dll (XmlReaderSettings.cs)
#define XmlSettings_IgnoreWhitespace             0x01
#define XmlSettings_IgnoreProcessingInstructions 0x02
#define XmlSettings_IgnoreComments               0x04

//--//

// XmlCharType definitions

#define XmlCharType_NCNameMask          0x0007

// XmlCharType_NCNameStartChar1 - 1-byte Name Start Characters
//      these are legal 1-byte utf-8 characters in the NCNameStartChar ranges
//      NCNameStartChar is defined by the XML spec as follow:
//      [A-Z] | "_" | [a-z] | [#xC0-#xD6] | [#xD8-#xF6] | [#xF8-#x2FF] | [#x370-#x37D] |
//      [#x37F-#x1FFF] | [#x200C-#x200D] | [#x2070-#x218F] | [#x2C00-#x2FEF] | 
//      [#x3001-#xD7FF] | [#xF900-#xFDCF] | [#xFDF0-#xFFFD] | [#x10000-#xEFFFF]
//
//      Note that since we don't support code x10000 and above, [#x10000-#xEFFFF] are
//      considered invalid in our system.
#define XmlCharType_NCNameStartChar1    0

// XmlCharType_NCNameChar1 - 1-byte Name Characters
//      These are additional legal 1-byte utf-8 characters that are allowed in
//      an Xml Name, but not as a start character.  
//      NCNameChar is defined by the XML spec as follow:
//      NameStartChar | "-" | "." | [0-9] | #xB7 | [#x0300-#x036F] | [#x203F-#x2040]
#define XmlCharType_NCNameChar1         1

// XmlCharType_NCNameChar2 - 2-byte Name and Name Start Characters
//      These are legal first byte of a 2-byte UTF-8 characters in both the
//      NCNameStartChar and NCNameChar ranges. No further validation is needed
//      other than together with the following byte, this form a legal UTF-8 
//      character (i.e. (byte2 & 0xC0) == 0x80 )
#define XmlCharType_NCNameChar2         2

// XmlCharType_NCNameChar2Maybe - Possible 2-byte Name and Name Start Characters
//      These might be legal first byte of a 2-byte UTF-8 characters in both the
//      NCNameStartChar and NCNameChar ranges. The content of byte 2 is required to
//      determine if the character is indeed a NCNameChar or NCNameStartChar
#define XmlCharType_NCNameChar2Maybe    3

// XmlCharType_NCNameChar3 - 3-byte Name and Name Start Characters
//      These are legal first byte of a 3-byte UTF-8 characters in both the
//      NCNameStartChar and NCNameChar ranges. No further validation is needed
//      other than together with the following 2 bytes, this form a legal UTF-8 
//      character (i.e. ((byte2 & 0xC0) == 0x80) && ((byte3 & 0xC0) == 0x80) )
#define XmlCharType_NCNameChar3         4

// XmlCharType_NCNameChar3Maybe - Possible 3-byte Name and Name Start Characters
//      These might be legal first byte of a 3-byte UTF-8 characters in both the
//      NCNameStartChar and NCNameChar ranges. The content of byte 2 and 3 are 
//      required to determine if the character is indeed a NCNameChar or 
//      NCNameStartChar.
#define XmlCharType_NCNameChar3Maybe    5

// XmlCharType_NCNameInvalid - Invalid Characters
//      These are byte codes that are not valid in any part of the name. Some are
//      restricted by the spec, others are invalid utf-8 byte codes.
#define XmlCharType_NCNameInvalid       6


#define XmlCharType_None            0x0000


// XmlCharType_Text - "Regular" Text Characters
// XmlCharType_AttributeDQuote - "Regular" Attribute Value Characters in ""
// XmlCharType_AttributeSQuote - "Regular" Attribute Value Characters in ''
// XmlCharType_CDATA - "Regular" characters in CDATA
// XmlCharType_Comment - "Regular" characters in comment
// XmlCharType_PI - "Regular" characters in PI
// XmlCharType_Whitespace - "Regular" characters in whitespace
//      These are all the legal characters that can be in the text, attribute ... nodes 
//      sans any special characters ("&", "<", and "]"). This is used to do a quick 
//      sanity check on the bytes. However, unlike the name characters,
//      we don't explicitly check for illegal unicode characters, so it's possible
//      that some illegal unicode characters will pass through.

#define XmlCharType_Text            0x0008
#define XmlCharType_AttributeDQuote 0x0010
#define XmlCharType_AttributeSQuote 0x0020
#define XmlCharType_CDATA           0x0040
#define XmlCharType_Comment         0x0080
#define XmlCharType_PI              0x0100
#define XmlCharType_Whitespace      0x0200

// XmlCharType_Space - Space Characters
//      These are characters that an XML document should treat as spaces.
//      It consists of 0x20 (space), 0x09 (tab), 0x0A (line feed), and 0x0D (carriage
//      return).
#define XmlCharType_Space           0x0400

#define XmlCharType_Slash           0x0800
#define XmlCharType_GT              0x1000
#define XmlCharType_Equal           0x2000
#define XmlCharType_Colon           0x4000
#define XmlCharType_Question        0x8000


#define XmlCharType_PrefixStopMark              XmlCharType_Space | XmlCharType_Slash | XmlCharType_GT | XmlCharType_Colon
#define XmlCharType_LocalNameStopMark           XmlCharType_Space | XmlCharType_Slash | XmlCharType_GT
#define XmlCharType_AttributePrefixStopMark     XmlCharType_Space | XmlCharType_Colon | XmlCharType_Equal
#define XmlCharType_AttributeLocalNameStopMark  XmlCharType_Space | XmlCharType_Equal
#define XmlCharType_PITargetStopMark            XmlCharType_Space | XmlCharType_Question

extern const CLR_UINT16 c_XmlCharType[];

//--//

// ShiftHelper is a helper sturcture used to keep track of end-of-line pre-processing
// (changing all \r\n in the doc to a single \n), as well as predefined entities
// (&gt; &lt; &amp; &apos; and &quot;) and character references (i.e. &#99; or &#x6b;)
// It uses the space originally occupied by the characters (ranging from 2 bytes for 
// \r\n to 4+ bytes for character references) to form a singly linked list, so that no
// memory allocation is required, and the shifting are done in place, and only after 
// the entire string has been proccessed by the ShiftBuffer() method.
//
// For the carriage return chain, the index of the first occurance is stored in 
// carRetChainStart (2 bytes), and subsequent indexes are stored in the position where
// previous \r\n was. For example, for the following string:
//
//           0   1   2   3   4   5   6   7   8   9  10  11  12  13  14
//         |---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
//         | a | a | a |\r |\n | a | a | a |\r |\n |\r |\n | a | a | a |
//         |---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
//  
// the chain would look like this:
//
//  start    0   1   2   3   4   5   6   7   8   9  10  11  12  13  14
// |-----| |---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
// |  3  | | a | a | a |   8   | a | a | a |   10  |   15  | a | a | a |
// |-----| |---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
//
// when ShiftBuffer() is called, we'll be able to use this information to create the
// final string in place:
//
//           0   1   2   3   4   5   6   7   8   9  10  11  12  13  14
//         |---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
//         | a | a | a |\n | a | a | a |\n |\n | a | a | a |...|...|...|
//         |---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
//
//
// Similarly for the character references chain, the index of the first occurance
// is stored in charRefChainStart (2 bytes), and subsequent indexes are stored in the
// position where the previous char ref was. In addition, we are also keeping track
// of the size of each char ref, as well as the resulting character.
// 
// For example, for the following strong:
//
//           0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19
//         |---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
//         | a | a | & | g | t | ; | a | a | & | q | u | o | t | ; | & | l | t | ; | a | a |  
//         |---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
//
// the chain would look like this:
//
//  start    0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19
// |-----| |---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
// |  3  | | a | a | > |   9   | 3 | a | a | " |   15  | 5 |...|...| < |   20  | 3 | a | a |  
// |-----| |---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
//                  ch   next   sz          ch   next   sz          ch   next   sz
//
// when ShiftBuffer() is called, we'll be able to use this information to create the
// final string in place:
//
//           0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19
//         |---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
//         | a | a | > | a | a | " | < | a | a |...|...|...|...|...|...|...|...|...|...|...|
//         |---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
//
//
struct CLR_RT_XmlShiftHelper
{
    CLR_UINT8  carRetChainStart[ 2 ];
    CLR_UINT8  charRefChainStart[ 2 ];
    CLR_UINT8* carRetChainNext;
    CLR_UINT8* charRefChainNext;

public:
    void Initialize();
    void SetNextCarRet ( CLR_UINT32 index, CLR_UINT8* next );
    void SetNextCharRef( CLR_UINT32 index, CLR_UINT8* next );
    CLR_UINT32 ShiftBuffer( CLR_UINT8* bufferStart, CLR_UINT8* bufferEnd, CLR_UINT8 carRetReplacement );
    void SaveRelativePositions   ( CLR_UINT8* value );
    void RestoreAbsolutePositions( CLR_UINT8* value );
};

struct CLR_RT_XmlCharRef
{
    static CLR_INT32 ParseCharRef( LPSTR buffer, CLR_UINT32 count, CLR_UINT32 index, CLR_RT_XmlShiftHelper* shiftHelper );
};

// XmlValueChunk and XmlValueChunks are helper structures that are used to store a singly
// linked list of value chunks to be concatenated once we have all of the text node.
// XmlValueChunks.Add() will allocated memory using CLR_RT_Memory::Allocate() (they are
// pinned and won't be GC'ed), the contents of the value, should already be pre-processed
// for end-of-line and character referneces. 
// CopyAllAndUnitialize() will copy all the contents in order to the specified destination
// and free up the memory that it allocated.
struct CLR_RT_XmlValueChunk
{
    CLR_UINT32            len;
    CLR_RT_XmlValueChunk* next;

    __inline CLR_UINT8* GetValueChunk() { return (CLR_UINT8*)&this[ 1 ]; }
};

struct CLR_RT_HeapBlock_XmlReader;

struct CLR_RT_XmlValueChunks
{
    CLR_UINT32            totalLen;
    CLR_RT_XmlValueChunk* head;
    CLR_RT_XmlValueChunk* tail;

public:
    void Initialize();
    HRESULT Add( CLR_UINT8* valueChunk, CLR_UINT32 len );
    HRESULT SetValueAndUninitialize( CLR_RT_HeapBlock_XmlReader* reader, CLR_UINT8* remainder, CLR_UINT32 remainderLen );
};

//--//

struct CLR_RT_HeapBlock_XmlReader;
union CLR_RT_XmlState;

typedef HRESULT (*XmlStateFn)( CLR_RT_XmlState* );
typedef void (*XmlStateCleanUpFn)( CLR_RT_XmlState* );
typedef void (*XmlStateInitFn)( CLR_RT_XmlState* );

#define XmlDocState_Start                  0
#define XmlDocState_DoneWithXmlDeclaration 1
#define XmlDocState_MainDocument           2
#define XmlDocState_DoneWithMainDocument   3

#define Library_system_xml_native_System_Xml_XmlReader__FIELD_STATIC__Xml            0
#define Library_system_xml_native_System_Xml_XmlReader__FIELD_STATIC__Xmlns          1
#define Library_system_xml_native_System_Xml_XmlReader__FIELD_STATIC__XmlNamespace   2
#define Library_system_xml_native_System_Xml_XmlReader__FIELD_STATIC__XmlnsNamespace 3

struct CLR_RT_XmlState_Base
{
    // stateFn is the main logic for each state, and is responsible to setting the next state
    XmlStateFn                  stateFn;
    // initFn is called when we returned from managed code, should be used to adjust any pointers
    XmlStateInitFn              initFn;
    // cleanupFn is called prior to returning from managed code when it fails with XML_E_NEED_MORE_DATA
    // should be used to adjust /shift the buffer to the desired location
    XmlStateCleanUpFn           cleanUpFn;
    
    CLR_UINT8*                  buffer;
    CLR_UINT8*                  bufferEnd;
    CLR_UINT8*                  bufferStart;
    CLR_RT_HeapBlock_XmlReader* reader;
    UINT32                      settings;
    UINT32                      docState;
    CLR_RT_Assembly*            systemXmlAssm;

    void ShiftBuffer( CLR_UINT8* start );

    __inline CLR_RT_HeapBlock_String* GetXml           () { return systemXmlAssm->GetStaticField( Library_system_xml_native_System_Xml_XmlReader__FIELD_STATIC__Xml            )->DereferenceString(); }
    __inline CLR_RT_HeapBlock_String* GetXmlns         () { return systemXmlAssm->GetStaticField( Library_system_xml_native_System_Xml_XmlReader__FIELD_STATIC__Xmlns          )->DereferenceString(); }
    __inline CLR_RT_HeapBlock_String* GetXmlNamespace  () { return systemXmlAssm->GetStaticField( Library_system_xml_native_System_Xml_XmlReader__FIELD_STATIC__XmlNamespace   )->DereferenceString(); }
    __inline CLR_RT_HeapBlock_String* GetXmlnsNamespace() { return systemXmlAssm->GetStaticField( Library_system_xml_native_System_Xml_XmlReader__FIELD_STATIC__XmlnsNamespace )->DereferenceString(); }
    
};

struct CLR_RT_XmlState_Start : public CLR_RT_XmlState_Base
{
public:
    static void Setup( CLR_RT_XmlState* st );
    static HRESULT Process( CLR_RT_XmlState* st );
};

enum CLR_RT_XmlNameType
{
    XmlNameType_Element          = 0,
    XmlNameType_PITarget         = 1,
    XmlNameType_Attribute        = 2,
    XmlnameType_XmlDeclAttribute = 3,
};

// InName state
// this is the state responsible for parsing qualified name in the xml document,
// it can be a name of the element or attribute, as speficied during setup with
// the isAttribute member. Note that if the name is longer than the length
// of the buffer (Currently 1024 bytes), it'll fail with XML_E_LIMIT_EXCEEDED
struct CLR_RT_XmlState_InName : public CLR_RT_XmlState_Base
{
private:
    enum Xml_ProcessNameStage
    {
        Xml_ProcessNameStage_Start       = 0,
        Xml_ProcessNameStage_InPrefix    = 1,
        Xml_ProcessNameStage_InLocalName = 2,
    };

    CLR_RT_XmlNameType   type;      // !!! MUST MATCH the offset of CLR_RT_XmlState_InAttributeValue->type and CLR_RT_XmlState_InAttribute->type !!!
    Xml_ProcessNameStage stage;
    CLR_UINT8*           name;
    CLR_UINT32           nameLen;
    CLR_UINT8*           prefix;
    CLR_UINT32           prefixLen;
    CLR_UINT8*           localName;
    CLR_UINT32           localNameLen;
    CLR_UINT16           prefixStopMark;
    CLR_UINT16           localNameStopMark;

public:
    static void Setup     ( CLR_RT_XmlState* st, CLR_UINT8* buffer, CLR_RT_XmlNameType type );
    static void Init      ( CLR_RT_XmlState* st );
    static void CleanUp   ( CLR_RT_XmlState* st );
    static HRESULT Process( CLR_RT_XmlState* st );

private:
    static HRESULT ProcessNameParts( CLR_RT_XmlState_InName* state, CLR_UINT32& nameLen, CLR_UINT16 stopsAt );
    static CLR_INT32 ProcessNCNameChar( CLR_UINT8* buffer, CLR_UINT32 count, bool isStartChar );
    static bool IsNCNameChar2( CLR_UINT8* buffer, bool isStartChar, bool isMaybe );
    static bool IsNCNameChar3( CLR_UINT8* buffer, bool isStartChar, bool isMaybe );
};

// InStartElement state
// Inside the start element tag, it'll send the parser to parse attributes if any, and
// return to managed code, once it has seen the entire start element tag
struct CLR_RT_XmlState_InStartElement : public CLR_RT_XmlState_Base
{
public:
    static void Setup     ( CLR_RT_XmlState* st );
    static HRESULT Process( CLR_RT_XmlState* st );
    
private:
    static HRESULT PrepReturn( CLR_RT_XmlState_InStartElement* state, bool isEmptyElement );
};

// BetweenElements state
// This is the grand central of all the states, it'll send the parser, depending on the content
// to text node, whitespace node, or a new tag (which can be start element, end element, comment
// PI, or CDATA)
struct CLR_RT_XmlState_BetweenElements : public CLR_RT_XmlState_Base
{
private:
    bool fromEndElement;
    
public:
    static void Setup     ( CLR_RT_XmlState* st, bool fromEndElement );
    static HRESULT Process( CLR_RT_XmlState* st );
};

// NewTag
// This state is responsible for differencitate between start element,
// end elements, comments, PIs, and CDATAs and send it to the approciate
// state.
struct CLR_RT_XmlState_NewTag : public CLR_RT_XmlState_Base
{
public:
    static void Setup     ( CLR_RT_XmlState* st );
    static HRESULT Process( CLR_RT_XmlState* st );
};

// InAttribute state
// this is the transition state between the attribute name, and the
// attribute value. It'll parse the "=" in between.
struct CLR_RT_XmlState_InAttribute : public CLR_RT_XmlState_Base
{
private:
    CLR_RT_XmlNameType type;        // !!! MUST MATCH the offset of CLR_RT_XmlState_InAttributeValue->type and CLR_RT_XmlState_InName->type !!!
    bool               seenEqual;
    
public:
    static void Setup     ( CLR_RT_XmlState* st );
    static HRESULT Process( CLR_RT_XmlState* st );
};

// InAttributeValue state
// the parser is inside the value portion of the attribute. It'll perform
// all the neccessary pre-processing for it (turning all tabs and line return
// to space, and char references). Note that if the attribute value is longer
// than the length of the buffer (currently 1024 bytes), it'll fail with 
// XML_E_LIMIT_EXCEEDED.
struct CLR_RT_XmlState_InAttributeValue : public CLR_RT_XmlState_Base
{
private:
    CLR_RT_XmlNameType    type;     // !!! MUST MATCH the offset of CLR_RT_XmlState_InAttribute->type and CLR_RT_XmlState_InName->type !!!
    CLR_UINT8*            value;
    CLR_RT_XmlShiftHelper shiftHelper;
    CLR_UINT8             quoteChar;
    CLR_UINT8             validAttributeChars;
    
public:
    static void Setup     ( CLR_RT_XmlState* st, CLR_UINT8* buffer, CLR_UINT8 quoteChar );
    static void Init      ( CLR_RT_XmlState* st );
    static void CleanUp   ( CLR_RT_XmlState* st );
    static HRESULT Process( CLR_RT_XmlState* st );
};

// InEndElement state
// this will parse the end element tag and make sure we have a matching
// start tag. It doesn't do the fancy QName parsing like start element, since
// we only need it for the purpose of comparsion.
struct CLR_RT_XmlState_InEndElement : public CLR_RT_XmlState_Base
{
private:
    CLR_UINT8* name;
    CLR_UINT32 nameLen;
    bool       doneWithName;

public:
    static void Setup     ( CLR_RT_XmlState* st, CLR_UINT8* buffer );
    static HRESULT Process( CLR_RT_XmlState* st );
    static void Init      ( CLR_RT_XmlState* st );
    static void CleanUp   ( CLR_RT_XmlState* st );
};

struct CLR_RT_XmlState_InProcessingInstruction : public CLR_RT_XmlState_Base
{   
public:
    static void Setup     ( CLR_RT_XmlState* st, CLR_UINT8* buffer );
    static HRESULT Process( CLR_RT_XmlState* st );
};

typedef HRESULT (*XmlStateInValueEnd)   ( CLR_RT_XmlState*, CLR_UINT8*&, bool& );
typedef HRESULT (*XmlStateInValueOthers)( CLR_RT_XmlState*, CLR_UINT8*&, bool& );

struct CLR_RT_XmlState_InValue : public CLR_RT_XmlState_Base
{
private:
    CLR_UINT8*            value;
    bool                  processOnly;
    CLR_RT_XmlShiftHelper shiftHelper;
    CLR_RT_XmlValueChunks valueChunks;
    XmlStateInValueEnd    endFn;
    XmlStateInValueOthers othersFn;
    CLR_UINT16            normalCharType;
    CLR_UINT8             endValueChar;
    
public:
    static void SetupPI        ( CLR_RT_XmlState* st, CLR_UINT8* buffer );
    static void SetupComment   ( CLR_RT_XmlState* st, CLR_UINT8* buffer );
    static void SetupCDATA     ( CLR_RT_XmlState* st, CLR_UINT8* buffer );
    static void SetupText      ( CLR_RT_XmlState* st, CLR_UINT8* buffer, bool fromWhitespace );
    static void SetupWhitespace( CLR_RT_XmlState* st, CLR_UINT8* buffer );

    static HRESULT Process( CLR_RT_XmlState* st );
    static void Init      ( CLR_RT_XmlState* st );
    static void CleanUp   ( CLR_RT_XmlState* st );

private:
    static void Setup( CLR_RT_XmlState* st, CLR_UINT8* buffer, bool processOnly );
    
    static HRESULT ProcessEndPI        ( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone );
    static HRESULT ProcessEndComment   ( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone );
    static HRESULT ProcessEndCDATA     ( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone );
    static HRESULT ProcessEndText      ( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone );
    static HRESULT ProcessEndWhitespace( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone );

    static HRESULT ProcessOthersInvalidChar( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone );
    static HRESULT ProcessOthersText       ( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone );
    static HRESULT ProcessOthersWhitespace ( CLR_RT_XmlState* st, CLR_UINT8*& buffer, bool& isDone );

    static HRESULT PrepReturn( CLR_RT_XmlState* st, CLR_UINT8*& buffer, CLR_UINT32 count, CLR_RT_XmlNodeType nodeType );
};

struct CLR_RT_XmlState_InXmlDeclaration : public CLR_RT_XmlState_Base
{
public:
    static void Setup     ( CLR_RT_XmlState* st );
    static HRESULT Process( CLR_RT_XmlState* st );
};

union CLR_RT_XmlState
{
    CLR_RT_XmlState_Base                    State;
    CLR_RT_XmlState_Start                   Start;
    CLR_RT_XmlState_InName                  InName;
    CLR_RT_XmlState_InStartElement          InStartElement;
    CLR_RT_XmlState_BetweenElements         BetweenElements;
    CLR_RT_XmlState_NewTag                  NewTag;
    CLR_RT_XmlState_InAttribute             InAttribute;
    CLR_RT_XmlState_InAttributeValue        InAttributeValue;
    CLR_RT_XmlState_InEndElement            InEndElement;
    CLR_RT_XmlState_InProcessingInstruction InProcessingInstruction;
    CLR_RT_XmlState_InXmlDeclaration        InXmlDeclaration;
    CLR_RT_XmlState_InValue                 InValue;
};

//--//

struct CLR_RT_HeapBlock_XmlNamespaceStack : private CLR_RT_HeapBlock_Stack
{
    HRESULT                  AddNamespace   ( CLR_RT_HeapBlock_String* prefix, CLR_RT_HeapBlock_String* namespaceURI );
    CLR_RT_HeapBlock_String* LookupNamespace( CLR_RT_HeapBlock_String* prefix );

    void PopScope( CLR_UINT32 nameSpaceCount );
};

//--//

#define Library_system_xml_native_System_Xml_XmlNameTable__FIELD___entries            1
#define Library_system_xml_native_System_Xml_XmlNameTable__FIELD___count              2
#define Library_system_xml_native_System_Xml_XmlNameTable__FIELD___mask               3
#define Library_system_xml_native_System_Xml_XmlNameTable__FIELD___hashCodeRandomizer 4
#define Library_system_xml_native_System_Xml_XmlNameTable__FIELD___tmp                5

struct CLR_RT_HeapBlock_XmlNameTable : public CLR_RT_HeapBlock
{
public:
    HRESULT Add( LPCSTR key, CLR_UINT32 length, CLR_RT_HeapBlock_String*& str, bool get = false );

private:
    HRESULT AddEntry( CLR_RT_HeapBlock_String*& str, CLR_INT32 hashCode );
    HRESULT Grow    (                                                   );

    __inline CLR_RT_HeapBlock_Array*  GetEntries   () { return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlNameTable__FIELD___entries            ].DereferenceArray(); }
    __inline CLR_INT32                GetCount     () { return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlNameTable__FIELD___count              ].NumericByRef().s4;  }
    __inline CLR_INT32                GetMask      () { return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlNameTable__FIELD___mask               ].NumericByRef().s4;  }
    __inline CLR_INT32                GetRandomizer() { return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlNameTable__FIELD___hashCodeRandomizer ].NumericByRef().s4;  }
    
    __inline void SetEntries( CLR_RT_HeapBlock_Array* entries ) { ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlNameTable__FIELD___entries ].SetObjectReference( entries ); }
    __inline void SetCount  ( CLR_INT32               count   ) { ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlNameTable__FIELD___count   ].SetInteger        ( count   ); }
    __inline void SetMask   ( CLR_INT32               mask    ) { ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlNameTable__FIELD___mask    ].SetInteger        ( mask    ); }
    __inline void SetTmp    ( CLR_RT_HeapBlock*       tmp     ) { ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlNameTable__FIELD___tmp     ].SetObjectReference( tmp     ); }
};


#define Library_system_xml_native_System_Xml_XmlNameTable_Entry__FIELD__Str      1
#define Library_system_xml_native_System_Xml_XmlNameTable_Entry__FIELD__HashCode 2
#define Library_system_xml_native_System_Xml_XmlNameTable_Entry__FIELD__Next     3

struct CLR_RT_HeapBlock_XmlNameTable_Entry : public CLR_RT_HeapBlock
{
public:
    __inline CLR_RT_HeapBlock_String*             GetStr     (){ return                                       ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlNameTable_Entry__FIELD__Str      ].DereferenceString();}
    __inline CLR_INT32                            GetHashCode(){ return                                       ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlNameTable_Entry__FIELD__HashCode ].NumericByRef().s4;  }
    __inline CLR_RT_HeapBlock_XmlNameTable_Entry* GetNext    (){ return (CLR_RT_HeapBlock_XmlNameTable_Entry*)((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlNameTable_Entry__FIELD__Next     ].Dereference();      }
    
    __inline void SetStr     ( CLR_RT_HeapBlock_String*             str      ){ ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlNameTable_Entry__FIELD__Str      ].SetObjectReference( str      ); }
    __inline void SetHashCode( CLR_INT32                            hashCode ){ ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlNameTable_Entry__FIELD__HashCode ].SetInteger        ( hashCode ); }
    __inline void SetNext    ( CLR_RT_HeapBlock_XmlNameTable_Entry* next     ){ ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlNameTable_Entry__FIELD__Next     ].SetObjectReference( next     ); }
};


#define Library_system_xml_native_System_Xml_XmlReader__FIELD___buffer            1
#define Library_system_xml_native_System_Xml_XmlReader__FIELD___offset            2
#define Library_system_xml_native_System_Xml_XmlReader__FIELD___length            3
#define Library_system_xml_native_System_Xml_XmlReader__FIELD___xmlNodes          4
#define Library_system_xml_native_System_Xml_XmlReader__FIELD___namespaces        5
#define Library_system_xml_native_System_Xml_XmlReader__FIELD___xmlSpaces         6
#define Library_system_xml_native_System_Xml_XmlReader__FIELD___xmlLangs          7
#define Library_system_xml_native_System_Xml_XmlReader__FIELD___nodeType          8
#define Library_system_xml_native_System_Xml_XmlReader__FIELD___value             9
#define Library_system_xml_native_System_Xml_XmlReader__FIELD___isEmptyElement    10
#define Library_system_xml_native_System_Xml_XmlReader__FIELD___attributes        11
#define Library_system_xml_native_System_Xml_XmlReader__FIELD___nameTable         12
#define Library_system_xml_native_System_Xml_XmlReader__FIELD___state             13
#define Library_system_xml_native_System_Xml_XmlReader__FIELD___isDone            14

struct CLR_RT_HeapBlock_XmlReader : public CLR_RT_HeapBlock
{
public:
    HRESULT Read();
    HRESULT Initialize( CLR_UINT32 settings, CLR_RT_Assembly* systemXmlAssm );

    __inline CLR_RT_HeapBlock_Stack*             GetXmlNodes         () { return (CLR_RT_HeapBlock_Stack*)            ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___xmlNodes   ].Dereference(); }
    __inline CLR_RT_HeapBlock_XmlNamespaceStack* GetNamespaces       () { return (CLR_RT_HeapBlock_XmlNamespaceStack*)((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___namespaces ].Dereference(); }
    __inline CLR_RT_HeapBlock_Stack*             GetXmlSpaces        () { return (CLR_RT_HeapBlock_Stack*)            ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___xmlSpaces  ].Dereference(); }
    __inline CLR_RT_HeapBlock_Stack*             GetXmlLangs         () { return (CLR_RT_HeapBlock_Stack*)            ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___xmlLangs   ].Dereference(); }
    __inline CLR_RT_HeapBlock_ArrayList*         GetAttributes       () { return (CLR_RT_HeapBlock_ArrayList*)        ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___attributes ].Dereference(); }
    __inline CLR_RT_HeapBlock_XmlNameTable*      GetNameTable        () { return (CLR_RT_HeapBlock_XmlNameTable*)     ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___nameTable  ].Dereference(); }

    __inline void SetNodeType      ( CLR_RT_XmlNodeType       nodeType       ) { ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___nodeType       ].SetInteger        ( nodeType       ); }
    __inline void SetValue         ( CLR_RT_HeapBlock_String* value          ) { ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___value          ].SetObjectReference( value          ); }
    __inline void SetIsEmptyElement( bool                     isEmptyElement ) { ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___isEmptyElement ].SetBoolean        ( isEmptyElement ); }
    __inline void SetIsDone        ( bool                     isDone         ) { ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___isDone         ].SetBoolean        ( isDone         ); }

private:
    __inline CLR_UINT8* GetBuffer() { return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___buffer ].DereferenceArray()->GetFirstElement(); }
    __inline CLR_INT32  GetOffset() { return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___offset ].NumericByRef().s4; }
    __inline CLR_INT32  GetLength() { return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___length ].NumericByRef().s4; }
    
    __inline CLR_RT_XmlState* GetState(){ return (CLR_RT_XmlState*)(((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___state ].DereferenceBinaryBlob()->GetData()); }
    
    __inline void SetOffset( CLR_INT32 offset ) { ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___offset ].SetInteger( offset ); }
    __inline void SetLength( CLR_INT32 length ) { ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader__FIELD___length ].SetInteger( length ); }
};


#define Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__Name         1
#define Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__NamespaceURI 2
#define Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__Prefix       3
#define Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__LocalName    4
#define Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__Flags        5

#define XmlNodeFlags_NamespaceCount 0x3FFFFFFF
#define XmlNodeFlags_HasXmlSpace    0x80000000
#define XmlNodeFlags_HasXmlLang     0x40000000

struct CLR_RT_HeapBlock_XmlNode : public CLR_RT_HeapBlock
{
public:

    void Clear();

    __inline void GetFlags( CLR_UINT32& namespaceCount, bool& hasXmlSpace, bool& hasXmlLang )
    {
        CLR_UINT32 flags = ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__Flags ].NumericByRef().u4;

        namespaceCount =  flags & XmlNodeFlags_NamespaceCount;
        hasXmlSpace    = (flags & XmlNodeFlags_HasXmlSpace) != 0;
        hasXmlLang     = (flags & XmlNodeFlags_HasXmlLang)  != 0;
    }
    
    __inline void SetXmlSpace            () { ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__Flags ].NumericByRef().u4 |= XmlNodeFlags_HasXmlSpace; }
    __inline void SetXmlLang             () { ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__Flags ].NumericByRef().u4 |= XmlNodeFlags_HasXmlLang;  }
    __inline void IncrementNamespaceCount() { ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__Flags ].NumericByRef().u4++;                           }
    __inline void ClearFlags             () { ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__Flags ].NumericByRef().u4 = 0;                         }
    
    __inline CLR_RT_HeapBlock_String* GetName  (){ return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__Name   ].DereferenceString(); }
    __inline CLR_RT_HeapBlock_String* GetPrefix(){ return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__Prefix ].DereferenceString(); }
    
    __inline void SetName        ( CLR_RT_HeapBlock_String* name         ){ ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__Name         ].SetObjectReference( name         ); }
    __inline void SetNamespaceURI( CLR_RT_HeapBlock_String* namespaceURI ){ ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__NamespaceURI ].SetObjectReference( namespaceURI ); }
    __inline void SetPrefix      ( CLR_RT_HeapBlock_String* prefix       ){ ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__Prefix       ].SetObjectReference( prefix       ); }
    __inline void SetLocalName   ( CLR_RT_HeapBlock_String* localName    ){ ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlNode__FIELD__LocalName    ].SetObjectReference( localName    ); }
};


#define Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__Name         1
#define Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__NamespaceURI 2
#define Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__Prefix       3
#define Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__LocalName    4
#define Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__Value        5

struct CLR_RT_HeapBlock_XmlAttribute : public CLR_RT_HeapBlock
{
public:
    __inline CLR_RT_HeapBlock_String* GetName        (){ return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__Name         ].DereferenceString(); }
    __inline CLR_RT_HeapBlock_String* GetNamespaceURI(){ return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__NamespaceURI ].DereferenceString(); }
    __inline CLR_RT_HeapBlock_String* GetPrefix      (){ return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__Prefix       ].DereferenceString(); }
    __inline CLR_RT_HeapBlock_String* GetLocalName   (){ return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__LocalName    ].DereferenceString(); }
    __inline CLR_RT_HeapBlock_String* GetValue       (){ return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__Value        ].DereferenceString(); }    
    
    __inline void SetName        ( CLR_RT_HeapBlock_String* name         ){ ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__Name         ].SetObjectReference( name         ); }
    __inline void SetNamespaceURI( CLR_RT_HeapBlock_String* namespaceURI ){ ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__NamespaceURI ].SetObjectReference( namespaceURI ); }
    __inline void SetPrefix      ( CLR_RT_HeapBlock_String* prefix       ){ ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__Prefix       ].SetObjectReference( prefix       ); }
    __inline void SetLocalName   ( CLR_RT_HeapBlock_String* localName    ){ ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__LocalName    ].SetObjectReference( localName    ); }
    __inline void SetValue       ( CLR_RT_HeapBlock_String* value        ){ ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_XmlAttribute__FIELD__Value        ].SetObjectReference( value        ); }
};


#define Library_system_xml_native_System_Xml_XmlReader_NamespaceEntry__FIELD__Prefix       1
#define Library_system_xml_native_System_Xml_XmlReader_NamespaceEntry__FIELD__NamespaceURI 2

struct CLR_RT_HeapBlock_XmlNamespaceEntry : public CLR_RT_HeapBlock
{
public:
    __inline CLR_RT_HeapBlock_String* GetPrefix      (){ return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_NamespaceEntry__FIELD__Prefix       ].DereferenceString(); }
    __inline CLR_RT_HeapBlock_String* GetNamespaceURI(){ return ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_NamespaceEntry__FIELD__NamespaceURI ].DereferenceString(); }

    __inline void SetPrefix      ( CLR_RT_HeapBlock_String* prefix       ){ ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_NamespaceEntry__FIELD__Prefix       ].SetObjectReference( prefix       ); }
    __inline void SetNamespaceURI( CLR_RT_HeapBlock_String* namespaceURI ){ ((CLR_RT_HeapBlock*)this)[ Library_system_xml_native_System_Xml_XmlReader_NamespaceEntry__FIELD__NamespaceURI ].SetObjectReference( namespaceURI ); }
};
//--//

#endif // _TINYCLR_XML_H_

