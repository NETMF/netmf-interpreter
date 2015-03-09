////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "Diagnostics.h"
#include "ManagedElementTypes_Win32.h"


// Table associating CLR types and string used in marshaling code.

static const CLR_RT_ManagedElementType::CLR_RT_TypeNamesRecord g_TypeNamesTable[] = 

{ {  DATATYPE_VOID     , "VOID"    , "void"  },
  {  DATATYPE_BOOLEAN  , "BOOLEAN" , "INT8"  }, 
  {  DATATYPE_CHAR     , "CHAR"    , "CHAR"  }, 
  {  DATATYPE_I1       , "I1"      , "INT8"  }, 
  {  DATATYPE_U1       , "U1"      , "UINT8" }, 
  {  DATATYPE_I2       , "I2"      , "INT16" }, 
  {  DATATYPE_U2       , "U2"      , "UINT16"}, 
  {  DATATYPE_I4       , "I4"      , "INT32" }, 
  {  DATATYPE_U4       , "U4"      , "UINT32"}, 
  {  DATATYPE_I8       , "I8"      , "INT64" }, 
  {  DATATYPE_U8       , "U8"      , "UINT64" }, 
  {  DATATYPE_R4       , "R4"      , "float" }, 
  {  DATATYPE_R8       , "R8"      , "double"}, 
  {  DATATYPE_STRING   , "STRING"  , "LPCSTR"}, 
  {  DATATYPE_BYREF    , "BYREF"   , "NONE"  }, 
  {  DATATYPE_VALUETYPE,  "NONE"   , "UNSUPPORTED_TYPE"  }, 
  {  DATATYPE_CLASS    ,  "NONE"   , "UNSUPPORTED_TYPE"  }, 
  {  DATATYPE_OBJECT   , "OBJECT"  , "UNSUPPORTED_TYPE"  }, 
};



CLR_RT_ManagedElementType::CLR_RT_ManagedElementType( CLR_DataType dataType  )
{
    m_dataType = dataType;
    m_pTypeData = NULL;
    
    // Iterate in table to get the name
    for ( int i = 0; i < ARRAYSIZE( g_TypeNamesTable ); i++ )
    {
        if ( m_dataType == g_TypeNamesTable[ i ].type )
        {
            m_pTypeData = &g_TypeNamesTable[ i ];
            break;
        }
    }
    
    // If m_pTypeData is NULL, means we get new type that metadata processor does not understand.
    // Print error to stdout and then 
    if ( m_pTypeData == NULL )
    {
        m_InvalidType.type = dataType;
        m_InvalidType.lpszTypeName   = "NONE";
        m_InvalidType.lpszNativeType = "UNSUPPORTED_TYPE";
        m_pTypeData = &m_InvalidType;
    }
}

CLR_RT_VectorOfManagedElements::~CLR_RT_VectorOfManagedElements()
{
    // Deletes all contained objects. 
    for ( iterator elem_Iter = begin(); elem_Iter != end(); elem_Iter++ )
    {   // Objects are of different type, so virtual destructor is invoked.
        delete *elem_Iter;
    }
}

string CLR_RT_ManagedElementType::GetParamIndexAsText( UINT32 index )
{
    CHAR szIndex[ 32 ];
    _itoa_s( index, szIndex, sizeof( szIndex ), 10 );
    return szIndex;
}


string CLR_RT_ManagedElementType::GetMarshalCodeBeforeNativeCall( bool bStaticMethod, UINT32 prmInd )
{
    // Get textual representation of parameter index like "0"
    string strIndex = GetParamIndexAsText( prmInd );

    // Generates line like "INT8 param1;"
    string strCode = "        " + GetVariableDecl() + " param" + strIndex + ";\n";
    
    // Generates line like "TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8( stack, 0, param0 ) );"
    strCode += string( "        " ) + "TINYCLR_CHECK_HRESULT( Interop_Marshal_" + GetNativeType() + "( stack, " + GetParamIndexAsText( prmInd + !bStaticMethod ) + ", param" + strIndex +" ) );\n"; 
    return strCode;
}

string CLR_RT_ManagedElementTypeByRef::GetMarshalCodeBeforeNativeCall( bool bStaticMethod, UINT32 prmInd )
{
    // Get textual representation of parameter index like "0"
    string strIndex = GetParamIndexAsText( prmInd );
    
    // Generates line like "INT8 param1;"
    string strCode = "        " + GetVariableDecl() + " param" + strIndex + ";\n";
    
    // Generates line like "UINT8 heapblock1[CLR_RT_HEAP_BLOCK_SIZE];"
    strCode += "        UINT8 heapblock" + strIndex + "[CLR_RT_HEAP_BLOCK_SIZE];\n";
    
    // Generates line like "TINYCLR_CHECK_HRESULT( Interop_Marshal_INT8_ByRef( stack, heapblock0, 0, param0 ) );"
    strCode += string( "        " ) + "TINYCLR_CHECK_HRESULT( Interop_Marshal_" + GetNativeType() + "_ByRef( stack, heapblock" + strIndex + ", " + GetParamIndexAsText( prmInd + !bStaticMethod ) + ", param" + strIndex +" ) );\n"; 
    return strCode;
}

string CLR_RT_ManagedElementTypeArray::GetMarshalCodeBeforeNativeCall( bool bStaticMethod, UINT32 prmInd )
{
    // Get textual representation of parameter index like "0"
    string strIndex = GetParamIndexAsText( prmInd );

    // Generates line like "CLR_RT_TypedArray_UINT8 param0;"
    string strCode = "        " + GetVariableDecl() + " param" + strIndex + ";\n";

    // Generates line like "TINYCLR_CHECK_HRESULT( Interop_Marshal_UINT8_ARRAY( stack, 0, pData0, arraySize0 ) );"
    strCode += "        TINYCLR_CHECK_HRESULT( Interop_Marshal_" + GetNativeType() + "_ARRAY( stack, " + GetParamIndexAsText( prmInd + !bStaticMethod ) + ", param" + strIndex + " ) );\n";
    return strCode;
}

string CLR_RT_ManagedElementTypeByRef::GetMashalCodeAfterNativeCall( UINT32 prmInd, bool bStaticMethod )
{
    // Get textual representation of parameter index like "0"
    string strBlockIndex = GetParamIndexAsText( prmInd );
    string strSaveIndex  = GetParamIndexAsText( bStaticMethod ? prmInd : prmInd + 1 );
    
    // Generates line like "TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock4, 4 ) );"
    return string( "        TINYCLR_CHECK_HRESULT( Interop_Marshal_StoreRef( stack, heapblock" ) + strBlockIndex + ", " + strSaveIndex + " ) );\n";
}
