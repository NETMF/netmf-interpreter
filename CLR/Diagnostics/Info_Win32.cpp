////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Diagnostics.h"
#include "ManagedElementTypes_Win32.h"

////////////////////////////////////////////////////////////////////////////////////////////////////

FILE* CLR_RT_Assembly::s_output = NULL;

void CLR_RT_Assembly::Dump_SetDevice( LPCWSTR szFileName )
{
    Dump_SetDevice( s_output, szFileName );
}

void CLR_RT_Assembly::Dump_SetDevice( FILE *&outputDeviceFile, LPCWSTR szFileName )
{
    if(szFileName)
    {        
        if(_wfopen_s( &outputDeviceFile, szFileName, L"w" ) != 0)
        {
            wprintf( L"Cannot open '%s' for writing!\n", szFileName );
        }
    }
}

void CLR_RT_Assembly::Dump_CloseDevice()
{
    Dump_CloseDevice( s_output );
}

void CLR_RT_Assembly::Dump_CloseDevice( FILE *&outputDeviceFile )
{
    if(outputDeviceFile)
    {
        fclose( outputDeviceFile ); outputDeviceFile = NULL;
    }
}


void CLR_RT_Assembly::Dump_Printf( const char *format, ... )
{
    va_list arg;

    va_start( arg, format );

    if(s_output)
    {
        vfprintf( s_output, format, arg );
    }
    else
    {
        CLR_Debug::PrintfV( format, arg );
    }

    va_end( arg );
}

void CLR_RT_Assembly::Dump_Printf( FILE *outputDeviceFile, const char *format, ... )
{
    va_list arg;

    va_start( arg, format );

    if(outputDeviceFile)
    {
        vfprintf( outputDeviceFile, format, arg );
    }
    else
    {
        CLR_Debug::PrintfV( format, arg );
    }

    va_end( arg );
}

//--//

void CLR_RT_Assembly::Dump_Indent( const CLR_RECORD_METHODDEF* md, size_t offset, size_t level )
{
    Dump_Printf( " IL_%04x(%04x):   ", offset, md->RVA + offset );

    while(level--)
    {
        Dump_Printf( "   " );
    }
}

void CLR_RT_Assembly::Dump( bool fNoByteCode )
{
    int i;

    Dump_Printf( "Assembly: %s (%d.%d.%d.%d)\n\n", m_szName, m_header->version.iMajorVersion, m_header->version.iMinorVersion, m_header->version.iBuildNumber, m_header->version.iRevisionNumber );

    Dump_Printf( "Size: AssemblyRef     %6d byte(s) %6d item(s) [Runtime: %6d]\n", m_pTablesSize[ TBL_AssemblyRef    ] * sizeof(CLR_RECORD_ASSEMBLYREF  ), m_pTablesSize[ TBL_AssemblyRef    ], m_pTablesSize[ TBL_AssemblyRef ] * sizeof(CLR_RT_AssemblyRef_CrossReference) );
    Dump_Printf( "Size: TypeRef         %6d byte(s) %6d item(s) [Runtime: %6d]\n", m_pTablesSize[ TBL_TypeRef        ] * sizeof(CLR_RECORD_TYPEREF      ), m_pTablesSize[ TBL_TypeRef        ], m_pTablesSize[ TBL_TypeRef     ] * sizeof(CLR_RT_TypeRef_CrossReference    ) );
    Dump_Printf( "Size: FieldRef        %6d byte(s) %6d item(s) [Runtime: %6d]\n", m_pTablesSize[ TBL_FieldRef       ] * sizeof(CLR_RECORD_FIELDREF     ), m_pTablesSize[ TBL_FieldRef       ], m_pTablesSize[ TBL_FieldRef    ] * sizeof(CLR_RT_FieldRef_CrossReference   ) );
    Dump_Printf( "Size: MethodRef       %6d byte(s) %6d item(s) [Runtime: %6d]\n", m_pTablesSize[ TBL_MethodRef      ] * sizeof(CLR_RECORD_METHODREF    ), m_pTablesSize[ TBL_MethodRef      ], m_pTablesSize[ TBL_MethodRef   ] * sizeof(CLR_RT_MethodRef_CrossReference  ) );
    Dump_Printf( "Size: TypeDef         %6d byte(s) %6d item(s) [Runtime: %6d]\n", m_pTablesSize[ TBL_TypeDef        ] * sizeof(CLR_RECORD_TYPEDEF      ), m_pTablesSize[ TBL_TypeDef        ], m_pTablesSize[ TBL_TypeDef     ] * sizeof(CLR_RT_TypeDef_CrossReference    ) );
    Dump_Printf( "Size: FieldDef        %6d byte(s) %6d item(s) [Runtime: %6d]\n", m_pTablesSize[ TBL_FieldDef       ] * sizeof(CLR_RECORD_FIELDDEF     ), m_pTablesSize[ TBL_FieldDef       ], m_pTablesSize[ TBL_FieldDef    ] * sizeof(CLR_RT_FieldDef_CrossReference   ) );
    Dump_Printf( "Size: MethodDef       %6d byte(s) %6d item(s) [Runtime: %6d]\n", m_pTablesSize[ TBL_MethodDef      ] * sizeof(CLR_RECORD_METHODDEF    ), m_pTablesSize[ TBL_MethodDef      ], m_pTablesSize[ TBL_MethodDef   ] * sizeof(CLR_RT_MethodDef_CrossReference  ) );
    Dump_Printf( "Size: Attributes      %6d byte(s) %6d item(s)\n"               , m_pTablesSize[ TBL_Attributes     ] * sizeof(CLR_RECORD_ATTRIBUTE    ), m_pTablesSize[ TBL_Attributes     ]                                                                             );
    Dump_Printf( "Size: TypeSpec        %6d byte(s) %6d item(s)\n"               , m_pTablesSize[ TBL_TypeSpec       ] * sizeof(CLR_RECORD_TYPESPEC     ), m_pTablesSize[ TBL_TypeSpec       ]                                                                             );
    Dump_Printf( "Size: Resources Files %6d byte(s) %6d item(s)\n"               , m_pTablesSize[ TBL_ResourcesFiles ] * sizeof(CLR_RECORD_RESOURCE_FILE), m_pTablesSize[ TBL_ResourcesFiles ]                                                                             );
    Dump_Printf( "Size: Resources       %6d byte(s) %6d item(s)\n"               , m_pTablesSize[ TBL_Resources      ] * sizeof(CLR_RECORD_RESOURCE     ), m_pTablesSize[ TBL_Resources      ]                                                                             );
    Dump_Printf( "Size: ResourcesData   %6d byte(s)\n"                           , m_pTablesSize[ TBL_ResourcesData  ]                                                                                                                                                     );
    Dump_Printf( "Size: Strings         %6d byte(s)\n"                           , m_pTablesSize[ TBL_Strings        ]                                                                                                                                                     );
    Dump_Printf( "Size: Signatures      %6d byte(s)\n"                           , m_pTablesSize[ TBL_Signatures     ]                                                                                                                                                     );
    Dump_Printf( "Size: ByteCode        %6d byte(s)\n\n\n"                       , m_pTablesSize[ TBL_ByteCode       ]                                                                                                                                                     );


#define ASMOFF(x) (UINT32)((size_t)x - (size_t)m_header)

    for(i=0; i<m_pTablesSize[ TBL_AssemblyRef ]; i++)
    {
        const CLR_RECORD_ASSEMBLYREF* p = GetAssemblyRef( i );

        Dump_Printf( "AssemblyRef: %04x[%08x] %s (%d.%d.%d.%d)\n", i, ASMOFF(p), GetString( p->name ), p->version.iMajorVersion, p->version.iMinorVersion, p->version.iBuildNumber, p->version.iRevisionNumber );
    }
    if(i) Dump_Printf( "\n" );

    for(i=0; i<m_pTablesSize[ TBL_TypeRef ]; i++)
    {
        const CLR_RECORD_TYPEREF* p = GetTypeRef( i );

        Dump_Printf( "TypeRef %04x[%08x] [Scope: %04x] %s %s\n", i, ASMOFF(p), p->scope, GetString( p->nameSpace ), GetString( p->name ) );
    }
    if(i) Dump_Printf( "\n" );

    for(i=0; i<m_pTablesSize[ TBL_FieldRef ]; i++)
    {
        const CLR_RECORD_FIELDREF* p = GetFieldRef( i );

        Dump_Printf   ( "FieldRef %04x[%08x] [Type: %04x] %s [", i, ASMOFF(p), p->container, GetString( p->name ) );
        Dump_Signature( p->sig );
        Dump_Printf   ( "]\n" );
    }
    if(i) Dump_Printf( "\n" );

    for(i=0; i<m_pTablesSize[ TBL_MethodRef ]; i++)
    {
        const CLR_RECORD_METHODREF* p = GetMethodRef( i );

        Dump_Printf   ( "MethodRef %04x[%08x] [Type: %04x] %s [", i, ASMOFF(p), p->container, GetString( p->name ) );
        Dump_Signature( p->sig );
        Dump_Printf   ( "]\n" );
    }
    if(i) Dump_Printf( "\n" );

    for(i=0; i<m_pTablesSize[ TBL_TypeDef ]; i++)
    {
        const CLR_RECORD_TYPEDEF* p = GetTypeDef( i );

        Dump_Printf( "TypeDef %04x[%08x] [Extends: %04x] [Enclosed: %04x] [Flags: %08x] %s %s\n", i, ASMOFF(p), p->extends, p->enclosingType, p->flags, GetString( p->nameSpace ), GetString( p->name ) );
    }
    if(i) Dump_Printf( "\n" );

    for(i=0; i<m_pTablesSize[ TBL_FieldDef ]; i++)
    {
        const CLR_RECORD_FIELDDEF* p = GetFieldDef( i );

        Dump_Printf    ( "FieldDef %04x[%08x] [Flags: %08x] ", i, ASMOFF(p), p->flags );
        Dump_FieldOwner( i                                                            );
        Dump_Printf    ( "::%s [", GetString( p->name )                               );
        Dump_Signature ( p->sig                                                       );
        Dump_Printf    ( "]\n"                                                        );
    }
    if(i) Dump_Printf( "\n" );

    for(i=0; i<m_pTablesSize[ TBL_MethodDef ]; i++)
    {
        const CLR_RECORD_METHODDEF* p = GetMethodDef( i );

        Dump_Printf     ( "MethodDef %04x[%08x] [Flags: %08x] [RVA: %04x] ", i, ASMOFF(p), p->flags, p->RVA );
        Dump_MethodOwner( i                                                                                 );
        Dump_Printf     ( "::%s [", GetString( p->name )                                                    );
        Dump_Signature  ( p->sig                                                                            );
        Dump_Printf     ( "]\n"                                                                             );
    }

    for(i=0; i<m_pTablesSize[ TBL_Attributes ]; i++)
    {
        const CLR_RECORD_ATTRIBUTE* p = GetAttribute( i );

        Dump_Printf( "Attribute %04x[%08x] [%04x:%04x] %04x %04x\n", i, ASMOFF(p), p->ownerType, p->ownerIdx, p->constructor, p->data );
    }
    if(i) Dump_Printf( "\n" );

    for(i=0; i<m_pTablesSize[ TBL_Resources ]-1; i++)
    {
        const CLR_RECORD_RESOURCE* p     = GetResource( i );
        const CLR_RECORD_RESOURCE* pNext = GetResource( i+1 );

        CLR_UINT16 size = pNext->offset - p->offset;
        Dump_Printf( "Resource %04x[%08x] [%08x:%1d] %04x %08x[%08x]\n", i, ASMOFF(p), p->id, p->kind, size, p->offset, ASMOFF(GetResourceData( p->offset )) );
    }
    if(i) Dump_Printf( "\n" );

#undef ASMOFF
}

//--//

void CLR_RT_Assembly::Dump_FieldOwner( CLR_UINT32 idx )
{
    const CLR_RECORD_TYPEDEF* td = GetTypeDef( 0 );

    for(CLR_IDX i=0; i<m_pTablesSize[TBL_TypeDef]; i++, td++)
    {
        if((idx >= td->iFields_First && idx < (CLR_UINT32)(td->iFields_First + td->iFields_Num)) ||
           (idx >= td->sFields_First && idx < (CLR_UINT32)(td->sFields_First + td->sFields_Num))  )
        {
            Dump_Token( CLR_TkFromType( TBL_TypeDef, i ) );
            return;
        }
    }
}

void CLR_RT_Assembly::Dump_MethodOwner( CLR_UINT32 idx )
{
    const CLR_RECORD_TYPEDEF* td = GetTypeDef( 0 );

    for(CLR_IDX i=0; i<m_pTablesSize[ TBL_TypeDef ]; i++, td++)
    {
        if(idx >= td->methods_First && idx < (CLR_UINT32)(td->methods_First + td->sMethods_Num + td->iMethods_Num + td->vMethods_Num))
        {
            Dump_Token( CLR_TkFromType( TBL_TypeDef, i ) );
            return;
        }
    }
}

void CLR_RT_Assembly::Dump_Token( CLR_UINT32 tk )
{
    CLR_UINT32 idx = CLR_DataFromTk( tk );

    switch(CLR_TypeFromTk(tk))
    {
    case TBL_AssemblyRef: { const CLR_RECORD_ASSEMBLYREF* p = GetAssemblyRef( idx );                                                                                                       Dump_Printf( "[%s]" ,                            GetString( p->name ) ); break; }
    case TBL_TypeRef    : { const CLR_RECORD_TYPEREF    * p = GetTypeRef    ( idx );                                                                                                       Dump_Printf( "%s.%s", GetString( p->nameSpace ), GetString( p->name ) ); break; }
    case TBL_FieldRef   : { const CLR_RECORD_FIELDREF   * p = GetFieldRef   ( idx );                                        Dump_Token( CLR_TkFromType( TBL_TypeRef, p->container     ) ); Dump_Printf( "::%s" ,                            GetString( p->name ) ); break; }
    case TBL_MethodRef  : { const CLR_RECORD_METHODREF  * p = GetMethodRef  ( idx );                                        Dump_Token( CLR_TkFromType( TBL_TypeRef, p->container     ) ); Dump_Printf( "::%s" ,                            GetString( p->name ) ); break; }
    case TBL_TypeDef    : { const CLR_RECORD_TYPEDEF    * p = GetTypeDef    ( idx ); if(p->enclosingType != CLR_EmptyIndex) Dump_Token( CLR_TkFromType( TBL_TypeDef, p->enclosingType ) ); Dump_Printf( "%s.%s", GetString( p->nameSpace ), GetString( p->name ) ); break; }
    case TBL_FieldDef   : { const CLR_RECORD_FIELDDEF   * p = GetFieldDef   ( idx );                                        Dump_FieldOwner ( idx );                                       Dump_Printf( "::%s" ,                            GetString( p->name ) ); break; }
    case TBL_MethodDef  : { const CLR_RECORD_METHODDEF  * p = GetMethodDef  ( idx );                                        Dump_MethodOwner( idx );                                       Dump_Printf( "::%s" ,                            GetString( p->name ) ); break; }
    case TBL_Strings    : { LPCSTR                        p = GetString     ( idx );                                                                                                       Dump_Printf( "'%s'" ,            p                                    ); break; }

    default:
        Dump_Printf( "[%08x]", tk );
    }
}

void CLR_RT_Assembly::Dump_Signature( CLR_SIG sig )
{
    const CLR_UINT8* p = GetSignature( sig );
    CLR_UINT32       len;

    CLR_CorCallingConvention cc = (CLR_CorCallingConvention)*p++;

    switch(cc & PIMAGE_CEE_CS_CALLCONV_MASK)
    {
    case PIMAGE_CEE_CS_CALLCONV_FIELD:
        Dump_Printf   ( "FIELD " );
        Dump_Signature( p        );
        break;

    case PIMAGE_CEE_CS_CALLCONV_LOCAL_SIG:
        break;

    case PIMAGE_CEE_CS_CALLCONV_DEFAULT:
        len = *p++;

        Dump_Printf   ( "METHOD " );
        Dump_Signature( p         );
        Dump_Printf   ( "("       );

        while(len-- > 0)
        {
            Dump_Printf   ( " " );
            Dump_Signature( p   );

            if(len) Dump_Printf( "," );
            else    Dump_Printf( " " );
        }
        Dump_Printf( ")" );
        break;
    }
}

void CLR_RT_Assembly::Dump_Signature( const CLR_UINT8*& p )
{
    CLR_DataType opt = CLR_UncompressElementType( p );

    switch(opt)
    {
        case DATATYPE_VOID      : Dump_Printf( "VOID"       );                           break;
        case DATATYPE_BOOLEAN   : Dump_Printf( "BOOLEAN"    );                           break;
        case DATATYPE_CHAR      : Dump_Printf( "CHAR"       );                           break;
        case DATATYPE_I1        : Dump_Printf( "I1"         );                           break;
        case DATATYPE_U1        : Dump_Printf( "U1"         );                           break;
        case DATATYPE_I2        : Dump_Printf( "I2"         );                           break;
        case DATATYPE_U2        : Dump_Printf( "U2"         );                           break;
        case DATATYPE_I4        : Dump_Printf( "I4"         );                           break;
        case DATATYPE_U4        : Dump_Printf( "U4"         );                           break;
        case DATATYPE_I8        : Dump_Printf( "I8"         );                           break;
        case DATATYPE_U8        : Dump_Printf( "U8"         );                           break;
        case DATATYPE_R4        : Dump_Printf( "R4"         );                           break;
        case DATATYPE_R8        : Dump_Printf( "R8"         );                           break;
        case DATATYPE_STRING    : Dump_Printf( "STRING"     );                           break;
        case DATATYPE_BYREF     : Dump_Printf( "BYREF "     ); Dump_Signature     ( p ); break;
        case DATATYPE_VALUETYPE : Dump_Printf( "VALUETYPE " ); Dump_SignatureToken( p ); break;
        case DATATYPE_CLASS     : Dump_Printf( "CLASS "     ); Dump_SignatureToken( p ); break;
        case DATATYPE_OBJECT    : Dump_Printf( "OBJECT"     );                           break;
        case DATATYPE_SZARRAY   : Dump_Printf( "SZARRAY "   ); Dump_Signature     ( p ); break;

        default                 : Dump_Printf( "[UNKNOWN: %08x]", opt );                 break;
    }

}

void CLR_RT_Assembly::Dump_SignatureToken( const CLR_UINT8*& p )
{
    CLR_UINT32 tk = CLR_TkFromStream( p );

    Dump_Printf( "[%08x]", tk );
}

//--//

void CLR_RT_TypeSystem::Dump( LPCWSTR szFileName, bool fNoByteCode )
{
    CLR_RT_Assembly::Dump_SetDevice( szFileName );

    TINYCLR_FOREACH_ASSEMBLY(*this)
    {
        pASSM->Dump( fNoByteCode );
    }
    TINYCLR_FOREACH_ASSEMBLY_END();

    CLR_RT_Assembly::Dump_CloseDevice();
}

//--//

static void RemoveDots( string& str )
{
    while(true)
    {
        string::size_type pos = str.find( '.' ); if(pos == str.npos) break;

        str[ pos ] = '_';
    }
}

static bool IncludeInStub( const CLR_RECORD_TYPEDEF* td )
{
    //
    // Only generate a stub for classes and value types.
    //
    if(td->flags & (CLR_RECORD_TYPEDEF::TD_Delegate | CLR_RECORD_TYPEDEF::TD_MulticastDelegate)) return false;

    switch(td->flags & CLR_RECORD_TYPEDEF::TD_Semantics_Mask)
    {
    case CLR_RECORD_TYPEDEF::TD_Semantics_Class:
    case CLR_RECORD_TYPEDEF::TD_Semantics_ValueType:
        return true;

    default:
        return false;
    }
}

static bool IncludeInStub( const CLR_RECORD_METHODDEF* md )
{
    return (md->RVA == CLR_EmptyIndex) && ((md->flags & md->MD_Abstract) == 0);
}

//--//
static const CHAR c_DO_NOT_EDIT_THIS_FILE_Header[] =
"//-----------------------------------------------------------------------------\n"
"//\n"
"//    ** DO NOT EDIT THIS FILE! **\n"
"//    This file was generated by a tool\n"
"//    re-running the tool will overwrite this file.\n"
"//\n"
"//-----------------------------------------------------------------------------\n"
"\n\n";

//--//
static const CHAR c_WARNING_FILE_OVERWRITE_Header[] =
"//-----------------------------------------------------------------------------\n"
"//\n"
"//                   ** WARNING! ** \n"
"//    This file was generated automatically by a tool.\n"
"//    Re-running the tool will overwrite this file.\n"
"//    You should copy this file to a custom location\n"
"//    before adding any customization in the copy to\n"
"//    prevent loss of your changes when the tool is\n"
"//    re-run.\n"
"//\n"
"//-----------------------------------------------------------------------------\n"
"\n\n";

static const CHAR c_Include_Header_Begin[] =
"\n"
"#include \"%S_native.h\"\n"
"\n"
"\n";

static const CHAR c_Include_Interop_h[] =
"#include <TinyCLR_Interop.h>\n";

//--//

static const CHAR c_Type_Begin[] =
"struct Library_%S_%s\n" \
"{\n";

static const CHAR c_Type_Field_Static[] =
"    static const int FIELD_STATIC__%s = %d;\n";

static const CHAR c_Type_Field_Instance[] =
"    static const int FIELD__%s = %d;\n";

static const CHAR c_Type_Method[] =
"    TINYCLR_NATIVE_DECLARE(%s);\n";

static const CHAR c_Type_End[] =
"\n"
"    //--//\n"
"\n"
"};\n\n";

//--//

static const CHAR c_Definition_Begin[] =
"#include \"%S.h\"\n"
"\n"
"\n"
"static const CLR_RT_MethodHandler method_lookup[] =\n"
"{\n";

static const CHAR c_Definition_Body[] = "    Library_%S_%s::%s,\n";

static const CHAR c_Definition_End[] =

"};\n"
"\n"
"const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_%s =\n"
"{\n"
"    \"%s\", \n"
"    0x%08X,\n"
"    method_lookup\n"
"};\n\n";

static const CHAR c_Method[] =
"\nHRESULT Library_%S_%s::%s( CLR_RT_StackFrame& stack )\n"
"{\n"
"%s"
"}\n";

static const CHAR c_MethodStub[] =
"    TINYCLR_HEADER();\n"
"\n"
"    TINYCLR_SET_AND_LEAVE(stack.NotImplementedStub());\n"
"\n"
"    TINYCLR_NOCLEANUP();\n";

//--//

static const CHAR c_Project_Gen_Project_Open[]  = "<Project DefaultTargets=\"Build\" ToolsVersion=\"3.5\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\n";
static const CHAR c_Project_Gen_Project_Close[] = "</Project>\n";

static const CHAR c_Project_Gen_Import_Project[] = "<Import Project=\"$(SPOCLIENT)\\tools\\targets\\Microsoft.SPOT.System.Targets\" />\n";

static const CHAR c_Project_Gen_Property_Group []= 
"  <PropertyGroup>\n"
"    <Directory>DeviceCode\\Targets\\Native\\Interop\\%S</Directory>\n"
"    <AssemblyName>%S</AssemblyName>\n"
"  </PropertyGroup>\n"
"  <Import Project=\"$(SPOCLIENT)\\tools\\targets\\Microsoft.SPOT.System.Settings\" />\n"
"  <PropertyGroup>\n"
"    <OutputType>Library</OutputType>\n"
"  </PropertyGroup>\n";

static const CHAR c_Project_Gen_Item_Group_Open[]  = "<ItemGroup>\n";
static const CHAR c_Project_Gen_Item_Group_Close[] = "</ItemGroup>\n";
//--//

void CLR_RT_Assembly::BuildClassName( const CLR_RECORD_TYPEREF* tr, string& cls_name, bool fEscape )
{
    cls_name.clear();

    if(tr->scope & 0x8000) // Flag for TypeRef
    {
        const CLR_RECORD_TYPEREF* trS = GetTypeRef( tr->scope & 0x7FFF );

        BuildClassName( GetTypeRef( tr->scope & 0x7FFF ), cls_name, fEscape );
        cls_name += ".";
    }
    else
    {
        const CLR_RECORD_ASSEMBLYREF* ar = GetAssemblyRef( tr->scope );

        cls_name  = GetString( ar->name );
        cls_name += ".";
    }

    if(tr->nameSpace != CLR_EmptyIndex)
    {
        cls_name += GetString( tr->nameSpace );
        cls_name += ".";
    }

    cls_name += GetString( tr->name );

    if(fEscape) RemoveDots( cls_name );
}

void CLR_RT_Assembly::BuildClassName( const CLR_RECORD_TYPEDEF* td, string& cls_name, bool fEscape )
{
    cls_name.clear();

    if(td->enclosingType != CLR_EmptyIndex)
    {
        BuildClassName( GetTypeDef( td->enclosingType ), cls_name, fEscape );
        cls_name += ".";
    }

    if(td->nameSpace != CLR_EmptyIndex)
    {
        cls_name += GetString( td->nameSpace );
        cls_name += ".";
    }

    cls_name += GetString( td->name );

    if(fEscape) RemoveDots( cls_name );
}

void CLR_RT_Assembly::BuildTypeName( const CLR_RECORD_TYPEDEF* td, std::string& type_name )
{
    type_name.clear();

    if(td->enclosingType != CLR_EmptyIndex)
    {
        BuildTypeName( GetTypeDef( td->enclosingType ), type_name );
        type_name += "_";
    }

    if(td->nameSpace != CLR_EmptyIndex)
    {
        std::string strNameSpace = GetString( td->nameSpace );
        
        // Avoid creation of extra dot if namespace is empty.
        if ( !strNameSpace.empty() )
        { 
            type_name += strNameSpace;
            type_name += ".";
        }
    }

    type_name += GetString( td->name );
}

void CLR_RT_Assembly::BuildParametersList( CLR_PMETADATA pMetaData, CLR_RT_VectorOfManagedElements &elemPtrArray )
{
    pMetaData++; // Skip Calling convention.
    CLR_UINT32    elemCount = *pMetaData++ + 1;
    
    //  Iterates until end of information with paramenters types.
    for ( CLR_UINT32 curElem = 0; curElem < elemCount; curElem++ )
    {
        CLR_RT_ManagedElementType *pNewElement = NULL;
        // Get next element type. 
        CLR_DataType opt = CLR_UncompressElementType( pMetaData );
        switch ( opt )
        {   
            // In case or array or reference we extract the type of element.
            case DATATYPE_BYREF  : pNewElement = new CLR_RT_ManagedElementTypeByRef( CLR_UncompressElementType( pMetaData ) ); break;
            case DATATYPE_SZARRAY: pNewElement = new CLR_RT_ManagedElementTypeArray( CLR_UncompressElementType( pMetaData ) ); break;
            
            case DATATYPE_VALUETYPE   :
            {  
                CLR_UINT32 token = CLR_TkFromStream( pMetaData );
                const CLR_RECORD_TYPEDEF *pTypeDef = GetTypeDef( CLR_DataFromTk( token ) );
                pNewElement = new CLR_RT_ManagedElementType( (CLR_DataType)pTypeDef->dataType );

            }
            break;
                
            // If it is not a reference or object - means basic type, we create CLR_RT_ManagedElementType
            default : pNewElement = new CLR_RT_ManagedElementType( opt ); 
        }
        // Adds pointer to new element to array.
        elemPtrArray.push_back( pNewElement );
    }
}


void CLR_RT_Assembly::BuildMethodName( const CLR_RECORD_METHODDEF* md, std::string& name, CLR_RT_StringMap& mapMethods )
{
    CHAR          buffer[ 256 ];
    CLR_PMETADATA p   = GetSignature( md->sig ); p++; // Skip Calling convention.
    CLR_UINT32    len = *p++ + 1;

    name = GetString( md->name ); RemoveDots( name );

    for(CLR_UINT32 a=0; a<len; a++)
    {
        bool fContinue = true;

        if(a == 0)
        {
            name += "___";

            if(md->flags & CLR_RECORD_METHODDEF::MD_Static)
            {
                name += "STATIC__";
            }
        }

        while(fContinue)
        {
            CLR_DataType opt = CLR_UncompressElementType( p );

            switch(opt)
            {
            case DATATYPE_VOID      : name += "VOID"     ; break;
            case DATATYPE_BOOLEAN   : name += "BOOLEAN"  ; break;
            case DATATYPE_CHAR      : name += "CHAR"     ; break;
            case DATATYPE_I1        : name += "I1"       ; break;
            case DATATYPE_U1        : name += "U1"       ; break;
            case DATATYPE_I2        : name += "I2"       ; break;
            case DATATYPE_U2        : name += "U2"       ; break;
            case DATATYPE_I4        : name += "I4"       ; break;
            case DATATYPE_U4        : name += "U4"       ; break;
            case DATATYPE_I8        : name += "I8"       ; break;
            case DATATYPE_U8        : name += "U8"       ; break;
            case DATATYPE_R4        : name += "R4"       ; break;
            case DATATYPE_R8        : name += "R8"       ; break;
            case DATATYPE_STRING    : name += "STRING"   ; break;
            case DATATYPE_BYREF     : name += "BYREF"    ; break;
            case DATATYPE_VALUETYPE :                    ; break;
            case DATATYPE_CLASS     :                    ; break;
            case DATATYPE_OBJECT    : name += "OBJECT"   ; break;
            case DATATYPE_SZARRAY   : name += "SZARRAY"  ; break;
            }

            fContinue = false;

            switch(opt)
            {
            case DATATYPE_BYREF  :
            case DATATYPE_SZARRAY:
                fContinue = true;
                name += "_";
                break;

            case DATATYPE_VALUETYPE:
            case DATATYPE_CLASS    :
                {
                    CLR_UINT32  tk = CLR_TkFromStream( p );
                    std::string cls;

                    switch(CLR_TypeFromTk(tk))
                    {
                    case TBL_TypeDef : BuildClassName( GetTypeDef( CLR_DataFromTk( tk ) ), cls, true ); break;
                    case TBL_TypeRef : BuildClassName( GetTypeRef( CLR_DataFromTk( tk ) ), cls, true ); break;
                    case TBL_TypeSpec: sprintf_s( buffer, ARRAYSIZE(buffer), "_%08X", tk ); cls = buffer; break;
                    }

                    while(true)
                    {
                        std::string::size_type pos = cls.find( '_' ); if(pos == cls.npos) break;

                        cls.erase( pos, 1 );
                    }

                    name += cls;
                }
                break;
            }
        }

        if(a != len-1) name += "__";
    }

    int num = mapMethods[name];
    if(num++)
    {
        sprintf_s( buffer, ARRAYSIZE(buffer), "_%d", num ); name += buffer;
    }
    mapMethods[name] = num;
}

static string ReplaceDotWithString( string strName, string strReplace )
{
    for ( string::iterator strp_Iter = strName.begin();
          strp_Iter != strName.end();
          strp_Iter++ 
        )
    {   
        if ( *strp_Iter == '.' )
        {
            string::iterator thisPos = strp_Iter;
            strName.replace( thisPos, ++strp_Iter, strReplace );
        }
    }
    return strName;
}

static string BuildHeaderDefMacro( LPCWSTR lpwzHeaderFilePath )
{
    WCHAR swDrive[ _MAX_DRIVE ], swDir[ _MAX_DIR ], swName[ _MAX_FNAME ], swExt[ _MAX_EXT ]; 

    // Breaks path into pieces to find the name of header file.
    _wsplitpath_s( lpwzHeaderFilePath, swDrive, ARRAYSIZE(swDrive), swDir, ARRAYSIZE(swDir), swName, ARRAYSIZE(swName), swExt, ARRAYSIZE(swExt) );
    
    // Convert to ASCII and build macro name.
    CHAR  szHeaderDefMacro[ _MAX_PATH + 5 ];
    sprintf_s( szHeaderDefMacro, sizeof( szHeaderDefMacro ), "_%S_H_", swName );
    
    // Converts file name to uppper case.
    _strupr_s( szHeaderDefMacro, sizeof( szHeaderDefMacro ) );
    return string( szHeaderDefMacro );
}

// Generate the call to native stublike like in example: 
// "INT64 AddValues( param0, param1, param3, hr );"

static string BuildCalltoNativeStub( bool bStaticMember, string strMethodName, string strAssemblyName, CLR_RT_VectorOfManagedElements &elemPtrArray )
{
    // Return type with function name like "INT64 AddValues("
    string strClassNames = ReplaceDotWithString( strAssemblyName, "::" );
    
    string strCall;
    // Add return value if native type is not "void". 
    // If we do not check for void type, we get code like "void retVal = ...".
    if ( !elemPtrArray[ 0 ]->IsVoidType() )
    {
        strCall += elemPtrArray[ 0 ]->GetNativeType() + " retVal = ";
    }
    
    // Add function call. We pre-pend class name to the function name
    strCall += strClassNames + "::" + strMethodName + "(";
    if ( !bStaticMember )
    {
        // For non-static member functions we add one more parameter - managed "fields"
        strCall += " pMngObj, ";
    }

    // Now parameters. 
    for ( UINT32 i = 1; i < elemPtrArray.size(); i++ )
    {
        // For each parameter add string like " INT8 param0,"
        strCall += " param" + CLR_RT_ManagedElementType::GetParamIndexAsText( i - 1 ) + ",";
    }

    // The last parameter is " hr );"
    strCall += " hr );";

    return strCall;
}

static string BuildDeclarationOfNativeStub( string strMethodName, string strAssemblyName, CLR_RT_VectorOfManagedElements &elemPtrArray, bool bStaticMethod )
{
    // Return type with function name like "INT64 AddValues("
    string strCall = elemPtrArray[ 0 ]->GetNativeType() + " ";
    // Include class name if passed - example : "INT64 TestBasicTypes::AddValues(
    if ( strAssemblyName.length() > 0 )
    {
        strCall += strAssemblyName + "::";
    }
    strCall += strMethodName + "( ";

    // For non-static methods we need to add CLR_RT_HeapBlock *pMngObj parameter.
    // The pMngObj is actually pointer to managed object
    if ( !bStaticMethod )
    {
        strCall += "CLR_RT_HeapBlock* pMngObj, ";
    }
    
    // Now parameters. 
    for ( UINT32 i = 1; i < elemPtrArray.size(); i++ )
    {
        // For each parameter add string like " INT8 param0,"
        strCall += elemPtrArray[ i ]->GetVariableDecl() + " param" + CLR_RT_ManagedElementType::GetParamIndexAsText( i - 1 ) + ", "; 
    }

    // The last parameter is " hr );"
    strCall += "HRESULT &hr )";

    return strCall;
}

static void BuildNameSpaceVector( string strNameSpace, vector<string> &vectNameSpace )
{
    // Need a copy of the string, since strtok actually modifies the string
    LPSTR lpszNameSpace = new char[ strNameSpace.length() + 1 ];
    strcpy_s( lpszNameSpace, strNameSpace.length() + 1, strNameSpace.c_str() ); 
    LPSTR  lpszContext;
    // Finds each token separated by "." and adds as separate string to vectNameSpace
    LPCSTR lpszName = strtok_s( lpszNameSpace, ".", &lpszContext );
    while ( lpszName != NULL )
    {
        vectNameSpace.push_back( string( lpszName ) );
        lpszName = strtok_s( NULL, ".", &lpszContext );
    }
    delete [] lpszNameSpace;
}

static std::wstring GetFNameWithExtFromFilePath( LPCWSTR lpwFilePath )

{ 
    // Split full path into pieces. 
    wchar_t szDrive[ _MAX_DRIVE ], szDir[ _MAX_DIR ], szFileName[ _MAX_FNAME ], szExt[ _MAX_EXT ];
    _wsplitpath_s( lpwFilePath, szDrive, ARRAYSIZE(szDrive), szDir, ARRAYSIZE(szDir), szFileName, ARRAYSIZE(szFileName), szExt, ARRAYSIZE(szExt) );
    // Adds file name and extention. 
    return std::wstring( szFileName ) + szExt;
}

static std::wstring GetDirPathFromFilePath( LPCWSTR lpwFilePath )

{ 
    // Split full path into pieces. 
    wchar_t szDrive[ _MAX_DRIVE ], szDir[ _MAX_DIR ], szFileName[ _MAX_FNAME ], szExt[ _MAX_EXT ];
    _wsplitpath_s( lpwFilePath, szDrive, ARRAYSIZE(szDrive), szDir, ARRAYSIZE(szDir), szFileName, ARRAYSIZE(szFileName), szExt, ARRAYSIZE(szExt) );
    // Adds drive name and directory name. 
    return std::wstring( szDrive ) + szDir;
}


static void UpdateCRC( UINT32& crc, LPCSTR str )
{
    crc = SUPPORT_ComputeCRC( str, (int)hal_strlen_s( str ), crc );
}

UINT32 CLR_RT_Assembly::GenerateSignatureForNativeMethods()
{
    UINT32                    crc = 0;
    const CLR_RECORD_TYPEDEF* td  = GetTypeDef( 0 );

    for(int i=0; i<m_pTablesSize[ TBL_TypeDef ]; i++, td++)
    {
        int totMethods = td->vMethods_Num + td->iMethods_Num + td->sMethods_Num;

        if(totMethods)
        {
            string                 cls_name; BuildClassName( td, cls_name, true );
            string                 name;
            CLR_RT_StringMap            mapMethods;
            const CLR_RECORD_METHODDEF* md = GetMethodDef( td->methods_First );

            for(int j=0; j<totMethods; j++, md++)
            {
                if(IncludeInStub( td ) && IncludeInStub( md ))
                {
                    BuildMethodName( md, name, mapMethods );

                    UpdateCRC( crc, m_szName         );
                    UpdateCRC( crc, cls_name.c_str() );
                    UpdateCRC( crc, name    .c_str() );
                }
                else
                {
                    UpdateCRC( crc, "NULL" );
                }
            }
        }
    }

    return crc;
}

bool CLR_RT_Assembly::AreInternalMethodsPresent( const CLR_RECORD_TYPEDEF* td )
{
    // If type in not included in stub, no code generation
    if ( !IncludeInStub( td ) )
    {
        return false;
    }

    // This type is included into stub. Check if it has internal methods 
    // that require marshaling code.
    const CLR_RECORD_METHODDEF* md = GetMethodDef( td->methods_First );
    int totMethods = td->vMethods_Num + td->iMethods_Num + td->sMethods_Num;
    // Iterates on all methods and checks if at least one requires marshaling code. 
    for ( int i = 0; i < totMethods; i++, md++ )
    {   // If at least one method require stub - return true
        if ( IncludeInStub( md ) )
        {
            return true;
        }
    }
    return false;
}

void CLR_RT_Assembly::GenerateSkeletonStubFieldsDef( const CLR_RECORD_TYPEDEF *pClsType, FILE *pFileStubHead, string strIndent, string strNameSpace )
{   
    if ( pClsType->iFields_Num )  // If fields are present.
    {   
        // Retrieve data with definition of the field.                                  
        const CLR_RECORD_FIELDDEF* fd = GetFieldDef( pClsType->iFields_First );

        // Iterates over all fields.
        string strCtorMembInit; 
        for ( int curFld =0; curFld < pClsType->iFields_Num; curFld++, fd++ )
        {
            // Signature parser is able to retrive type of the field from fd->sig 
            CLR_RT_SignatureParser sigParser;
            sigParser.Initialize_FieldDef( this, fd );
            
            // Now sigParser.m_sig is actually points to the type of the field. 
            // Construct CLR_RT_ManagedElementType which retrive name of type from ID.
            CLR_PMETADATA pMetaData = sigParser.m_sig;
            CLR_DataType opt = CLR_UncompressElementType( pMetaData );

            if ( opt == DATATYPE_VALUETYPE )
            {  
                CLR_UINT32 token = CLR_TkFromStream( pMetaData );
                CLR_UINT32 dataToken = CLR_DataFromTk( token );
                const CLR_RECORD_TYPEDEF *pTypeDef = GetTypeDef( CLR_DataFromTk( token ) );
                opt =  (CLR_DataType)pTypeDef->dataType;

            }
            
            CLR_RT_ManagedElementType elemType( opt );
            
            // Generates function that retrieves the managed field
            // First function declaration
            string strFieldDef = strIndent + "static " + elemType.GetNativeType() + "& Get_" + GetString( fd->name ) + "( CLR_RT_HeapBlock* pMngObj )    ";
            // Now generates the body of the function. 
            strFieldDef += "{ return Interop_Marshal_GetField_" + elemType.GetNativeType() + "( pMngObj, " + strNameSpace + "::FIELD__" + GetString( fd->name ) + " ); }\n\n";


            // Print field definition to header file.
            Dump_Printf( pFileStubHead, strFieldDef.c_str() );
        }
    }
}

void CLR_RT_Assembly::GenerateSkeletonStubCode( LPCWSTR szFilePath, FILE *pFileDotNetProj )
{
    /*
    ** Creates 3 files. 
    ** 1.   Create <assembly>_<type>_mshl.cpp, with the marshalling code.
    ** 2.   Create <assembly>_<type>.h  with the declarations of native stub functions for developer.
    ** 3.   Create <assembly>_<type>.cpp  with the defintioins of native stub functions for developer.                                                              
    */
    
    const CLR_RECORD_TYPEDEF* td = GetTypeDef( 0 );
    WCHAR   rgFiles[ 2*MAX_PATH ];
    LPCWSTR szName;
    CHAR    szCharName[ MAX_PATH ];
   
    //   Finds the file name name out of szFilePath 
    for ( szName = szFilePath + wcslen( szFilePath ); szName != szFilePath; szName-- )
    {
        if ( *(szName-1) == '\\' || *(szName-1) == '/' ) break;
    }
    sprintf_s( szCharName, sizeof( szCharName ), "%S", szName );

    // Iterates over all classes in assembly.
    for ( int i=0; i < m_pTablesSize[ TBL_TypeDef ]; i++, td++ )
    {
        // Calculate count of methods in the class.
        int totMethods = td->vMethods_Num + td->iMethods_Num + td->sMethods_Num;
        
        // Enter "if" statement only if internal methods requiring marshaling code are present.
        // Thus we skip classes that do not have internal methods.
        if ( totMethods && AreInternalMethodsPresent( td ) )
        {
            string                 cls_name; 
            BuildClassName( td, cls_name, true );
            CLR_RT_StringMap            mapMethods;
            const CLR_RECORD_METHODDEF* md = GetMethodDef( td->methods_First );
            
            // File with marshaling code. ( *.cpp )
            swprintf( rgFiles, ARRAYSIZE(rgFiles), L"%s_%S_mshl.cpp", szFilePath, cls_name.c_str() ); 
            FILE *pFileMshlSrc = NULL;  Dump_SetDevice(  pFileMshlSrc, rgFiles);
            // Adds file to the dotNetMF.proj file.
            Dump_Printf( pFileDotNetProj, "<Compile Include=\"%S\" />\n", GetFNameWithExtFromFilePath( rgFiles ).c_str() );

            // File with declaration ( *.h ) of native stub functions for developer
            swprintf( rgFiles, ARRAYSIZE(rgFiles), L"%s_%S.h", szFilePath, cls_name.c_str() ); 
            FILE *pFileStubHead = NULL;  Dump_SetDevice( pFileStubHead, rgFiles );
            // Adds file to the dotNetMF.proj file.
            Dump_Printf( pFileDotNetProj, "<HFile Include=\"%S\" />\n", GetFNameWithExtFromFilePath( rgFiles ).c_str() );
            string strHeaderDefineMacro = BuildHeaderDefMacro( rgFiles );
            
            // File with definition ( *.cpp )  of native stub functions for developer
            swprintf( rgFiles, ARRAYSIZE(rgFiles), L"%s_%S.cpp", szFilePath, cls_name.c_str() ); 
            FILE *pFileStubSrc = NULL;  Dump_SetDevice( pFileStubSrc, rgFiles );
            // Adds file to the dotNetMF.proj file.
            Dump_Printf( pFileDotNetProj, "<Compile Include=\"%S\" />\n", GetFNameWithExtFromFilePath( rgFiles ).c_str() );


            // Put Comments header into each file
            Dump_Printf( pFileMshlSrc,  c_DO_NOT_EDIT_THIS_FILE_Header );
            Dump_Printf( pFileStubSrc,  c_WARNING_FILE_OVERWRITE_Header );
            Dump_Printf( pFileStubHead, c_WARNING_FILE_OVERWRITE_Header );

            // In header file prevention of multople inclusion.
            Dump_Printf( pFileStubHead, "#ifndef %s\n", strHeaderDefineMacro.c_str() );
            Dump_Printf( pFileStubHead, "#define %s\n\n", strHeaderDefineMacro.c_str() );


            // Include file with Interop table declaration like "InteropSample_Native.h"
            Dump_Printf( pFileMshlSrc, "#include \"%S.h\"\n", szName );
            // Include file with with declaration ( *.h ) of native stub functions for developer
            Dump_Printf( pFileMshlSrc, "#include \"%S_%s.h\"\n\n", szName, cls_name.c_str() );

            // Include file with Interop table declaration like "InteropSample_Native.h"
            Dump_Printf( pFileStubSrc, "#include \"%S.h\"\n", szName );
            // Include file with with declaration ( *.h ) of native stub functions for developer
            Dump_Printf( pFileStubSrc, "#include \"%S_%s.h\"\n\n", szName, cls_name.c_str() );
            
            //*************************  Add enclosure of stub functions declarations with namespace ***********
            string strNameSpace;
            BuildTypeName( td, strNameSpace );
            string strUsingNamesSpace  = "using namespace ";
            vector<string> vectNameSpace;
            BuildNameSpaceVector( strNameSpace, vectNameSpace );
            string strMngClassName = (*--vectNameSpace.end()).c_str();

            // Array of closing brackets for namespaces.
            vector<string> vectClosingBrackets;
            string strIndent;
            // Add namespace strings
            for ( UINT32 i = 0; i < vectNameSpace.size() - 1; i++, strIndent += "    " )
            {
                string strSingleNameSpace   = strIndent + "namespace " + vectNameSpace[ i ].c_str();
                string strSingleOpenBracket = strIndent + "{";
                Dump_Printf( pFileStubHead, "%s\n%s\n", strSingleNameSpace.c_str(), strSingleOpenBracket.c_str() );
                // Insert closing bracket into array. Added at the end of the file.
                vectClosingBrackets.insert( vectClosingBrackets.begin(), strIndent + "}\n" );
                
                // Build "using namespace" string. Example : "using namespace Microsoft::SPOT::InteropSample;"
                strUsingNamesSpace += vectNameSpace[ i ].c_str();
                // Adds "::" separator between namespaces and ";" after last namespace.
                strUsingNamesSpace += i < vectNameSpace.size() - 2 ? "::" : ";\n\n";
            }
            // Add string with "struct"
            Dump_Printf( pFileStubHead, "%sstruct %s\n%s{\n", strIndent.c_str(), strMngClassName.c_str(), strIndent.c_str() );
            vectClosingBrackets.insert( vectClosingBrackets.begin(), strIndent + "};\n" );
            strIndent += "    ";
            
            // Add definitions of fields (data members ) to the declaration of native structure.
            Dump_Printf( pFileStubHead, ( strIndent + "// Helper Functions to access fields of managed object\n" ).c_str() );
            GenerateSkeletonStubFieldsDef( td, pFileStubHead, strIndent, "Library_" + string( szCharName ) + "_" + cls_name );

            //************** Add using namespace statement to source files. 
            Dump_Printf( pFileMshlSrc, strUsingNamesSpace.c_str() );
            Dump_Printf( pFileStubSrc, strUsingNamesSpace.c_str() );

            // Add methods declarations and definitions. 
            Dump_Printf( pFileStubHead, ( strIndent + "// Declaration of stubs. These functions are implemented by Interop code developers\n" ).c_str() );
            for ( int j=0; j < totMethods; j++, md++)   // Iterates over all methods in class. 
            {
                if ( IncludeInStub( md ) )   // Test if it is internal method. 
                {
                    // Test if this is static function of the class.
                    bool bStaticMethod( (md->flags & CLR_RECORD_METHODDEF::MD_Static) != 0 );
                    string strNativeStubName = ReplaceDotWithString( GetString( md->name ), "_" );
                    
                    // elemPtrArray represents parameters to the funtion. One element for each parameter. 
                    CLR_RT_VectorOfManagedElements elemPtrArray; 
                    BuildParametersList( GetSignature( md->sig ), elemPtrArray );

                    // Here we generate implementation for the function that receives the managed stack frame.
                    // This function retrieves and marshal all parameters from stack frame, then calls stub 
                    // implemented by native code developer.
                    
                    // Name for the function that receives managed stack frame
                    string  strFuncName;
                    BuildMethodName( md, strFuncName, mapMethods );

                    // Generate the implementation for the function.
                    // Everything that goes inside of curly brackets.
                    string  strFuncImpl = "    TINYCLR_HEADER(); hr = S_OK;\n    {\n";

                    // If function is not static - we need to retrieve adresses ( in form of references ) of all member variables.
                    if ( !bStaticMethod )
                    {   // Retrieve pointer to first field ( member varialbe ) in the managed class.
                        const CLR_RECORD_FIELDDEF* fd = GetFieldDef( td->iFields_First );
                        strFuncImpl += "        CLR_RT_HeapBlock* pMngObj = Interop_Marshal_RetrieveManagedObject( stack );\n\n"; 
                        strFuncImpl += "        FAULT_ON_NULL(pMngObj);\n\n"; 

                    }

                    // Add marshaling code for each parameter:
                    for ( UINT32 i = 1; i < elemPtrArray.size(); i++ )
                    {
                        // For non-static member function zero param is "this" pointer. 
                        // bStaticMethod is used to adjust index for slot used by "this" pointer.
                        strFuncImpl += elemPtrArray[ i ]->GetMarshalCodeBeforeNativeCall( bStaticMethod, i - 1 ) + "\n";
                    }

                    // Add call for native stub implemented by developer.
                    // Example: "INT32 retVal = TestBasicTypes::Add_Values_Array_double( param0, arraySize0, hr );"
                    strFuncImpl += "        " + BuildCalltoNativeStub( bStaticMethod, strNativeStubName.c_str(), strMngClassName, elemPtrArray ) + "\n";

                    //****************** Declaration and definition for the stub method. ******************
                    // Example: "INT32 TestBasicTypes::Add_Values_Array_double( param0, arraySize0, hr );"
                    string strNativeStub = BuildDeclarationOfNativeStub( strNativeStubName.c_str(), string(), elemPtrArray, bStaticMethod );
                    
                    // Creates string with "static" modifier.
                    Dump_Printf( pFileStubHead, "%sstatic %s;\n", strIndent.c_str(), strNativeStub.c_str() );
                    
                    // Add empty definition of the stub implemented by developer to *.cpp file.
                    Dump_Printf( pFileStubSrc, BuildDeclarationOfNativeStub( strNativeStubName.c_str(), strMngClassName, elemPtrArray, bStaticMethod ).c_str() );
                    // Now the stub implementation. 
                    // If function is not void, we just declare return value equal to zero and return it.
                    if ( !elemPtrArray[ 0 ]->IsVoidType() )
                    {
                        Dump_Printf( pFileStubSrc, "\n{\n    %s retVal = 0; \n    return retVal;\n}\n\n", elemPtrArray[ 0 ]->GetNativeType().c_str() );
                    }
                     else
                    { // In case of void function we just have empty "{}" implementation
                        Dump_Printf( pFileStubSrc,"\n{\n}\n\n" );
                    }
                    //******************  Add Check for hr returned by stub function:
                    strFuncImpl += "        TINYCLR_CHECK_HRESULT( hr );\n";

                    // Adds call to SetResult, like "SetResult_UINT16( stack, retVal );
                    // We need to set result only if managed method is not void.
                    if ( !elemPtrArray[ 0 ]->IsVoidType() )
                    {
                        strFuncImpl += "        SetResult_" + elemPtrArray[ 0 ]->GetNativeType() + "( stack, retVal );\n\n";
                    }

                    // Add code for storing of references.
                    for ( UINT32 i = 1; i < elemPtrArray.size(); i++ )
                    {
                        strFuncImpl += elemPtrArray[ i ]->GetMashalCodeAfterNativeCall( i - 1, bStaticMethod );
                    }
                    
                    // Closing TINYCLR_NOCLEANUP(); macro. Defines label and returns "hr"
                    strFuncImpl += "    }\n    TINYCLR_NOCLEANUP();\n";
                    
                    // Writes function definition file. 
                    Dump_Printf( pFileMshlSrc, c_Method, szName, cls_name.c_str(), strFuncName.c_str(), strFuncImpl.c_str() );
                }
            }

            // All the closing curly brackets for namespaces in the header file with stub functions declarations.
            for ( UINT32 i = 0; i < vectClosingBrackets.size(); i++ )
            {
                Dump_Printf( pFileStubHead, vectClosingBrackets[ i ].c_str() );
            }
            
            // #endif for prevention of multiple inclusion of header file
            Dump_Printf( pFileStubHead, "#endif  //%s\n", strHeaderDefineMacro.c_str() );

            // Close all 3 files
            Dump_CloseDevice( pFileMshlSrc );
            Dump_CloseDevice( pFileStubSrc );
            Dump_CloseDevice( pFileStubHead );
        }
    }
}

static void FixNameToValidIdentifier( LPCWSTR szOriginalName, LPWSTR szFixedName, int cchFixedName )

{
    // We need to exclude not alpha numeric characters from szFilePath and szProjectName.
    // Copy characters until '\0' is reached or end of rgFilePath
    for ( int i = 0; i < cchFixedName; i++ )
    {
       szFixedName[ i ] = szOriginalName[i];
       // If end of string reached - exit
       if (szFixedName[ i ] == L'\0')
       {
           break;
       }
       
       // Replace not alpha numeric with '_'
       if (!iswalnum( szFixedName[  i] ))
       {
          szFixedName[ i ] = L'_';
       }
    }

    // First letter should not be digit for identifier. Change it to '_'
    if (szFixedName[ 0 ] != L'\0' && iswdigit( szFixedName[ 0 ] ))
    {
       szFixedName[ 0 ] = L'_';
    }
}


void CLR_RT_Assembly::GenerateSkeletonFromComplientNames( LPCWSTR szFilePath, LPCWSTR szProjectName )
{
    WCHAR   rgFiles[ 2*MAX_PATH ] = {0};
    LPCWSTR szName = NULL;

    // Creates local variable with name of assembly ( like Microsoft.SPOT.Graphics ).
    string strAssemblyIDName = ReplaceDotWithString( m_szName, "_" );

    // Opens file for dotNetMF.proj project file 
    swprintf( rgFiles, ARRAYSIZE(rgFiles), L"%sdotNetMF.proj", GetDirPathFromFilePath( szFilePath ).c_str() );
    FILE *pFileDotNetProj = NULL;  Dump_SetDevice(  pFileDotNetProj, rgFiles);
    // Start of XML dotNetMF.proj file
    Dump_Printf( pFileDotNetProj, c_Project_Gen_Project_Open );
    // Property group with project name. 
    Dump_Printf( pFileDotNetProj, c_Project_Gen_Property_Group, szProjectName, szProjectName );
    // Opens Item Group in dotNetmf.proj
    Dump_Printf( pFileDotNetProj, c_Project_Gen_Item_Group_Open );


    //
    // 1) Create <assembly>.h, with the structs declarations.
    //
    {   swprintf( rgFiles, ARRAYSIZE(rgFiles), L"%s.h", szFilePath ); Dump_SetDevice( rgFiles );
        // Adds the file name to the dotNetMF.proj file.
        Dump_Printf( pFileDotNetProj, "<HFile Include=\"%S\" />\n", GetFNameWithExtFromFilePath( rgFiles ).c_str() );


       //Allow szFilePath to point to a path, not just a prefix
        for(szName = szFilePath + wcslen( szFilePath ); szName != szFilePath; szName--)
        {
            if(*(szName-1) == '\\' || *(szName-1) == '/') break;
        }
        
        // Build string for the symbol to prevent multiple inclusion of header
        // Example "_SPOT_INTEROPSAMPLE_NATIVE_H_"
        string strHeaderDefineMacro = BuildHeaderDefMacro( rgFiles );
        
        // Copy write header.
        Dump_Printf( c_DO_NOT_EDIT_THIS_FILE_Header );
        
        // Prevent multiple inclusion of header file.
        Dump_Printf( "#ifndef %s\n",   strHeaderDefineMacro.c_str() );
        Dump_Printf( "#define %s\n\n", strHeaderDefineMacro.c_str() );
        
        // Add #include <TinyCLR_Interop.h>\n;
        Dump_Printf( c_Include_Interop_h );

        int                       iStaticFields = 0;
        const CLR_RECORD_TYPEDEF* td            = GetTypeDef( 0 );

        for(int i=0; i<m_pTablesSize[ TBL_TypeDef ]; i++, td++)
        {
            if(IncludeInStub( td ))
            {
                string cls_name; BuildClassName( td, cls_name, true );
                
                // Look at the class name. 
                // If class name starts from <PrivateImplementationDetails>,
                // then we need to exclude this class as actually this is static data object 
                // described in metadata.
                if ( NULL != strstr( cls_name.c_str(), "<PrivateImplementationDetails>" ) )
                {
                    // Go to the next class. This metadata describes global variable, not a type
                    continue;
                }



                bool        fSeen = false;

                if(td->sFields_Num)
                {
                    const CLR_RECORD_FIELDDEF* fd = GetFieldDef( td->sFields_First );

                    for(int j=0; j<td->sFields_Num; j++, fd++)
                    {
                        if(fSeen == false) { Dump_Printf( c_Type_Begin, szName, cls_name.c_str() ); fSeen = true; }

                        Dump_Printf( c_Type_Field_Static, GetString( fd->name ), j + iStaticFields );
                    }

                    Dump_Printf( "\n" );
                }

                if(td->iFields_Num)
                {
                    const CLR_RECORD_FIELDDEF* fd = GetFieldDef( td->iFields_First );

                    for(int j=0; j<td->iFields_Num; j++, fd++)
                    {
                        if(fSeen == false) { Dump_Printf( c_Type_Begin, szName, cls_name.c_str() ); fSeen = true; }

                        Dump_Printf( c_Type_Field_Instance, GetString( fd->name ), m_pCrossReference_FieldDef[ j+td->iFields_First ].m_offset );
                    }

                    Dump_Printf( "\n" );
                }

                {
                    int totMethods = td->vMethods_Num + td->iMethods_Num + td->sMethods_Num;

                    if(totMethods)
                    {
                        string                 name;
                        CLR_RT_StringMap            mapMethods;
                        const CLR_RECORD_METHODDEF* md = GetMethodDef( td->methods_First );

                        for(int j=0; j<totMethods; j++, md++)
                        {
                            if(IncludeInStub( md ))
                            {
                                BuildMethodName( md, name, mapMethods );

                                if(fSeen == false) { Dump_Printf( c_Type_Begin, szName, cls_name.c_str() ); fSeen = true; }

                                Dump_Printf( c_Type_Method, name.c_str() );
                            }
                        }
                    }
                }

                if(fSeen) { Dump_Printf( c_Type_End ); }
            }

            iStaticFields += td->sFields_Num;
        }

        Dump_Printf( "\n\nextern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_%s;\n\n", strAssemblyIDName.c_str() );
        // #endif for multiple inclusion of file.
        Dump_Printf( "#endif  //%s\n", strHeaderDefineMacro.c_str() );
        Dump_CloseDevice();
    }

    //
    // 2) Create <assembly>.cpp, with the lookup definition.
    //
    {   
        swprintf( rgFiles, ARRAYSIZE(rgFiles), L"%s.cpp", szFilePath ); Dump_SetDevice( rgFiles );
        // Adds source file to dotnetmf.proj
        Dump_Printf( pFileDotNetProj, "<Compile Include=\"%S\" />\n", GetFNameWithExtFromFilePath( rgFiles ).c_str() );

        Dump_Printf( c_DO_NOT_EDIT_THIS_FILE_Header );
        Dump_Printf( c_Definition_Begin, szName );

        const CLR_RECORD_TYPEDEF* td = GetTypeDef( 0 );

        for(int i=0; i<m_pTablesSize[ TBL_TypeDef ]; i++, td++)
        {
            int totMethods = td->vMethods_Num + td->iMethods_Num + td->sMethods_Num;

            if(totMethods)
            {
                string                 cls_name; BuildClassName( td, cls_name, true );
                string                 name;
                CLR_RT_StringMap            mapMethods;
                const CLR_RECORD_METHODDEF* md = GetMethodDef( td->methods_First );

                for(int j=0; j<totMethods; j++, md++)
                {
                    if(IncludeInStub( td ) && IncludeInStub( md ))
                    {
                        BuildMethodName( md, name, mapMethods );

                        Dump_Printf( c_Definition_Body, szName, cls_name.c_str(), name.c_str() );
                    }
                    else
                    {
                        Dump_Printf( "    NULL,\n" );
                    }
                }
            }
        }

        {
            CLR_RECORD_ASSEMBLY* header = (CLR_RECORD_ASSEMBLY*)m_header;

            header->nativeMethodsChecksum = GenerateSignatureForNativeMethods();
            header->ComputeCRC();
        }

        // Generates code for global variable of type CLR_RT_NativeAssemblyData.
        Dump_Printf( c_Definition_End, 
                     strAssemblyIDName.c_str(),
                     m_szName, 
                     m_header->nativeMethodsChecksum 
                   );

        Dump_CloseDevice();
    }

    
    // Generates marshaling and stub code sources and headers
    GenerateSkeletonStubCode( szFilePath, pFileDotNetProj );
    
    // Closes Item Group in dotNetmf.proj 
    Dump_Printf( pFileDotNetProj, c_Project_Gen_Item_Group_Close );
    // Add "Import Project" to the dotNetmf.proj  
    Dump_Printf( pFileDotNetProj, c_Project_Gen_Import_Project );
    // Closes XML "</Project>\n"
    Dump_Printf( pFileDotNetProj, c_Project_Gen_Project_Close );
    // Closes project file.
    Dump_CloseDevice( pFileDotNetProj );
}

void CLR_RT_Assembly::GenerateSkeleton( LPCWSTR szFilePath, LPCWSTR szProjectName )

{
    // Buffers for updated szFilePath and szProjectName.
    wchar_t   rgFileName   [ MAX_PATH+1 ] = {0};
    wchar_t   rgProjectName[ MAX_PATH+1 ] = {0};
    
    // Fix strFileName  to make it complient with C identifier.
    std::wstring strFileName = GetFNameWithExtFromFilePath( szFilePath );
    FixNameToValidIdentifier( strFileName.c_str(), rgFileName, ARRAYSIZE( rgFileName ) - 1 );
    std::wstring strFilePath = GetDirPathFromFilePath( szFilePath ) + rgFileName;

    // Fix szProjectName to make it complient with C identifier.  
    FixNameToValidIdentifier( szProjectName, rgProjectName, ARRAYSIZE( rgProjectName ) - 1 );

    // Now calls the main generation functions with fixed names. ( Mostly dots converted to '_' )
    GenerateSkeletonFromComplientNames( strFilePath.c_str(), rgProjectName );
}

void CLR_RT_Assembly::BuildMethodName_Legacy( const CLR_RECORD_METHODDEF* md, std::string& name, CLR_RT_StringMap& mapMethods )
{
    CHAR          buffer[256];
    CLR_PMETADATA p   = GetSignature( md->sig ); p++; // Skip Calling convention.
    CLR_UINT32    len = *p++ + 1;

    name = GetString( md->name ); RemoveDots( name );

    for(CLR_UINT32 a=0; a<len; a++)
    {
        bool fContinue = true;

        if(a == 0)
        {
            name += "___";

            if(md->flags & CLR_RECORD_METHODDEF::MD_Static)
            {
                name += "STATIC__";
            }
        }

        while(fContinue)
        {
            CLR_DataType opt = CLR_UncompressElementType( p );

            switch(opt)
            {
            case DATATYPE_VOID      : name += "VOID"     ; break;
            case DATATYPE_BOOLEAN   : name += "BOOLEAN"  ; break;
            case DATATYPE_CHAR      : name += "CHAR"     ; break;
            case DATATYPE_I1        : name += "I1"       ; break;
            case DATATYPE_U1        : name += "U1"       ; break;
            case DATATYPE_I2        : name += "I2"       ; break;
            case DATATYPE_U2        : name += "U2"       ; break;
            case DATATYPE_I4        : name += "I4"       ; break;
            case DATATYPE_U4        : name += "U4"       ; break;
            case DATATYPE_I8        : name += "I8"       ; break;
            case DATATYPE_U8        : name += "U8"       ; break;
            case DATATYPE_R4        : name += "R4"       ; break;
            case DATATYPE_R8        : name += "R8"       ; break;
            case DATATYPE_STRING    : name += "STRING"   ; break;
            case DATATYPE_BYREF     : name += "BYREF"    ; break;
            case DATATYPE_VALUETYPE :                    ; break;
            case DATATYPE_CLASS     :                    ; break;
            case DATATYPE_OBJECT    : name += "OBJECT"   ; break;
            case DATATYPE_SZARRAY   : name += "SZARRAY"  ; break;
            }

            fContinue = false;

            switch(opt)
            {
            case DATATYPE_BYREF  :
            case DATATYPE_SZARRAY:
                fContinue = true;
                name += "_";
                break;

            case DATATYPE_VALUETYPE:
            case DATATYPE_CLASS    :
                {
                    CLR_UINT32  tk = CLR_TkFromStream( p );
                    std::string cls;

                    switch(CLR_TypeFromTk(tk))
                    {
                    case TBL_TypeDef : BuildClassName( GetTypeDef( CLR_DataFromTk( tk ) ), cls, true ); break;
                    case TBL_TypeRef : BuildClassName( GetTypeRef( CLR_DataFromTk( tk ) ), cls, true ); break;
                    case TBL_TypeSpec: sprintf_s( buffer, ARRAYSIZE(buffer), "_%08X", tk ); cls = buffer; break;
                    }

                    while(true)
                    {
                        std::string::size_type pos = cls.find( '_' ); if(pos == cls.npos) break;

                        cls.erase( pos, 1 );
                    }

                    name += cls;
                }
                break;
            }
        }

        if(a != len-1) name += "__";
    }

    int num = mapMethods[name];
    if(num++)
    {
        sprintf_s( buffer, ARRAYSIZE(buffer), "_%d", num ); name += buffer;
    }
    mapMethods[name] = num;
}


void CLR_RT_Assembly::GenerateSkeleton_Legacy( LPCWSTR szFileName, LPCWSTR szProjectName )
{
    WCHAR   rgFiles[2*MAX_PATH];
    LPCWSTR szName;

    // Creates local variable with name of assembly ( like Microsoft.SPOT.Graphics ).
    std::string strAssemblyIDName = m_szName;
    // Replaces "." with "_" so the assembly name could be part of C++ identifier name.
    for ( std::basic_string<char>::iterator strp_Iter = strAssemblyIDName.begin();
          strp_Iter != strAssemblyIDName.end();
          strp_Iter++ 
        )
    {   if ( *strp_Iter == '.' )
        {
            *strp_Iter = '_';
        }
    }

    //
    // 1) Create <assembly>.h, with the structs declarations.
    //
    {   swprintf( rgFiles, ARRAYSIZE(rgFiles), L"%s.h", szFileName ); Dump_SetDevice( rgFiles );

        //Allow szFileName to point to a path, not just a prefix
        for(szName = szFileName + wcslen( szFileName ); szName != szFileName; szName--)
        {
            if(*(szName-1) == '\\' || *(szName-1) == '/') break;
        }

        Dump_Printf( c_WARNING_FILE_OVERWRITE_Header );

        string strHeaderDefineMacro = BuildHeaderDefMacro( rgFiles );

        Dump_Printf( "#ifndef %s\n", strHeaderDefineMacro.c_str() );
        Dump_Printf( "#define %s\n\n", strHeaderDefineMacro.c_str() );

        int                       iStaticFields = 0;
        const CLR_RECORD_TYPEDEF* td            = GetTypeDef( 0 );

        for(int i=0; i<m_pTablesSize[TBL_TypeDef]; i++, td++)
        {
            if(IncludeInStub( td ))
            {
                std::string cls_name; BuildClassName( td, cls_name, true );
                bool        fSeen = false;

                if(td->sFields_Num)
                {
                    const CLR_RECORD_FIELDDEF* fd = GetFieldDef( td->sFields_First );

                    for(int j=0; j<td->sFields_Num; j++, fd++)
                    {
                        if(fSeen == false) { Dump_Printf( c_Type_Begin, szName, cls_name.c_str() ); fSeen = true; }

                        Dump_Printf( c_Type_Field_Static, GetString( fd->name ), j + iStaticFields );
                    }

                    Dump_Printf( "\n" );
                }

                if(td->iFields_Num)
                {
                    const CLR_RECORD_FIELDDEF* fd = GetFieldDef( td->iFields_First );

                    for(int j=0; j<td->iFields_Num; j++, fd++)
                    {
                        if(fSeen == false) { Dump_Printf( c_Type_Begin, szName, cls_name.c_str() ); fSeen = true; }

                        Dump_Printf( c_Type_Field_Instance, GetString( fd->name ), m_pCrossReference_FieldDef[j+td->iFields_First].m_offset );
                    }

                    Dump_Printf( "\n" );
                }

                {
                    int totMethods = td->vMethods_Num + td->iMethods_Num + td->sMethods_Num;

                    if(totMethods)
                    {
                        std::string                 name;
                        CLR_RT_StringMap            mapMethods;
                        const CLR_RECORD_METHODDEF* md = GetMethodDef( td->methods_First );

                        for(int j=0; j<totMethods; j++, md++)
                        {
                            if(IncludeInStub( md ))
                            {
                                BuildMethodName( md, name, mapMethods );

                                if(fSeen == false) { Dump_Printf( c_Type_Begin, szName, cls_name.c_str() ); fSeen = true; }

                                Dump_Printf( c_Type_Method, name.c_str() );
                            }
                        }
                    }
                }

                if(fSeen) { Dump_Printf( c_Type_End ); }
            }

            iStaticFields += td->sFields_Num;
        }

        //Dump_Printf( c_Declaration_End, strAssemblyIDName.c_str() );

        Dump_Printf( "#endif  //%s\n", strHeaderDefineMacro.c_str() );
        Dump_CloseDevice();
    }

    //
    // 2) Create <assembly>.cpp, with the lookup definition.
    //
    {   
        swprintf( rgFiles, ARRAYSIZE(rgFiles), L"%s.cpp", szFileName ); Dump_SetDevice( rgFiles );

        Dump_Printf( c_Definition_Begin, szProjectName );

        const CLR_RECORD_TYPEDEF* td = GetTypeDef( 0 );

        for(int i=0; i<m_pTablesSize[TBL_TypeDef]; i++, td++)
        {
            int totMethods = td->vMethods_Num + td->iMethods_Num + td->sMethods_Num;

            if(totMethods)
            {
                std::string                 cls_name; BuildClassName( td, cls_name, true );
                std::string                 name;
                CLR_RT_StringMap            mapMethods;
                const CLR_RECORD_METHODDEF* md = GetMethodDef( td->methods_First );

                for(int j=0; j<totMethods; j++, md++)
                {
                    if(IncludeInStub( td ) && IncludeInStub( md ))
                    {
                        BuildMethodName( md, name, mapMethods );

                        Dump_Printf( c_Definition_Body, szName, cls_name.c_str(), name.c_str() );
                    }
                    else
                    {
                        Dump_Printf( "    NULL,\n" );
                    }
                }
            }
        }

        {
            CLR_RECORD_ASSEMBLY* header = (CLR_RECORD_ASSEMBLY*)m_header;

            header->nativeMethodsChecksum = GenerateSignatureForNativeMethods();
            header->ComputeCRC();
        }

        // Creates global variable of type CLR_RT_NativeAssemblyData.
        Dump_Printf( c_Definition_End, 
                     strAssemblyIDName.c_str(),
                     m_szName, 
                     m_header->nativeMethodsChecksum 
                   );


        Dump_CloseDevice();
    }

    //
    // 3) Create <assembly>_<type>.cpp, with the type definition.
    //
    {
        const CLR_RECORD_TYPEDEF* td = GetTypeDef( 0 );

        for(int i=0; i<m_pTablesSize[TBL_TypeDef]; i++, td++)
        {
            int totMethods = td->vMethods_Num + td->iMethods_Num + td->sMethods_Num;

            if(totMethods && IncludeInStub( td ))
            {
                std::string                 cls_name; BuildClassName( td, cls_name, true );
                std::string                 name;
                CLR_RT_StringMap            mapMethods;
                const CLR_RECORD_METHODDEF* md = GetMethodDef( td->methods_First );
                bool fFirst = true;

                for(int j=0; j<totMethods; j++, md++)
                {
                    if(IncludeInStub( md ))
                    {
                        if(fFirst)
                        {
                            swprintf( rgFiles, ARRAYSIZE(rgFiles), L"%s_%S.cpp", szFileName, cls_name.c_str() ); Dump_SetDevice( rgFiles );
                            
                            Dump_Printf( c_WARNING_FILE_OVERWRITE_Header );
                            
                            Dump_Printf( c_Include_Header_Begin, szProjectName );

                            fFirst = false;
                        }
                        BuildMethodName( md, name, mapMethods );

                        Dump_Printf( c_Method, szName, cls_name.c_str(), name.c_str(), c_MethodStub );
                    }
                }

                if(!fFirst)
                {
                    Dump_CloseDevice();
                }
            }
        }
    }
}
