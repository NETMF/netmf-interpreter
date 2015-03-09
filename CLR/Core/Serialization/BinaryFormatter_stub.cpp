////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "..\Core.h"

bool CLR_RT_BinaryFormatter::SerializationEnabled()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    return false;
}

CLR_RT_HeapBlock* CLR_RT_BinaryFormatter::TypeHandler::FixDereference( CLR_RT_HeapBlock* v )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    return NULL;
}

CLR_RT_HeapBlock* CLR_RT_BinaryFormatter::TypeHandler::FixNull( CLR_RT_HeapBlock* v )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    return NULL;
}

//--//

HRESULT CLR_RT_BinaryFormatter::TypeHandler::TypeHandler_Initialize( CLR_RT_BinaryFormatter* bf, SerializationHintsAttribute* hints, CLR_RT_TypeDescriptor* expected )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_BinaryFormatter::TypeHandler::SetValue( CLR_RT_HeapBlock* v )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

//--//

int CLR_RT_BinaryFormatter::TypeHandler::SignatureRequirements()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    return 0;
}

bool CLR_RT_BinaryFormatter::TypeHandler::CompareTypes( CLR_RT_TypeDescriptor* left, CLR_RT_TypeDescriptor* right )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    return true;
}

CLR_DataType CLR_RT_BinaryFormatter::TypeHandler::GetDataType( CLR_RT_TypeDescriptor* type )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    return (CLR_DataType)0;
}

CLR_UINT32 CLR_RT_BinaryFormatter::TypeHandler::GetSizeOfType( CLR_RT_TypeDescriptor* type )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    return 0;
}

bool CLR_RT_BinaryFormatter::TypeHandler::GetSignOfType( CLR_RT_TypeDescriptor* type )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    return true;
}

//--//

HRESULT CLR_RT_BinaryFormatter::TypeHandler::EmitSignature( int& res )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_BinaryFormatter::TypeHandler::EmitSignature_Inner( int mask, CLR_RT_TypeDescriptor* type, CLR_RT_HeapBlock* value )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

//--//

HRESULT CLR_RT_BinaryFormatter::TypeHandler::ReadSignature( int& res )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

//--//

HRESULT CLR_RT_BinaryFormatter::TypeHandler::EmitValue( int& res )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_BinaryFormatter::TypeHandler::ReadValue( int& res )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

//--//

HRESULT CLR_RT_BinaryFormatter::TypeHandler::TrackObject( int& res )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_BinaryFormatter::State::CreateInstance( CLR_RT_BinaryFormatter* parent, SerializationHintsAttribute* hints, CLR_RT_HeapBlock* type )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_BinaryFormatter::State::CreateInstance( CLR_RT_BinaryFormatter* parent, SerializationHintsAttribute* hints, CLR_RT_TypeDescriptor* type )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

void CLR_RT_BinaryFormatter::State::DestroyInstance()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
}

//--//

HRESULT CLR_RT_BinaryFormatter::State::FindHints( SerializationHintsAttribute& hints, const CLR_RT_TypeDef_Instance& cls )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_BinaryFormatter::State::FindHints( SerializationHintsAttribute& hints, const CLR_RT_FieldDef_Instance& fld )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

//--//

HRESULT CLR_RT_BinaryFormatter::State::AssignAndFixBoxing( CLR_RT_HeapBlock& dst )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_BinaryFormatter::State::GetValue()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_BinaryFormatter::State::SetValueAndDestroyInstance()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

//////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_BinaryFormatter::State::Advance()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

//--//

HRESULT CLR_RT_BinaryFormatter::State::AdvanceToTheNextField()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

//--//

HRESULT CLR_RT_BinaryFormatter::State::AdvanceToTheNextElement()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

////////////////////////////////////////////////////////////////////////////////////////////////////

HRESULT CLR_RT_BinaryFormatter::CreateInstance( CLR_UINT8* buf, int len, CLR_RT_BinaryFormatter*& res )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

void CLR_RT_BinaryFormatter::DestroyInstance()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
}

HRESULT CLR_RT_BinaryFormatter::Advance()
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

//--//

void CLR_RT_BinaryFormatter::PrepareForGC( void* data )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
}

HRESULT CLR_RT_BinaryFormatter::Serialize( CLR_RT_HeapBlock& refData, CLR_RT_HeapBlock& object, CLR_RT_HeapBlock* cls, CLR_UINT32 flags )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_BinaryFormatter::Deserialize( CLR_RT_HeapBlock& refData, CLR_RT_HeapBlock& object, CLR_RT_HeapBlock* cls, CLR_UINT32* unknownType, CLR_UINT32 flags )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_BinaryFormatter::Deserialize( CLR_RT_HeapBlock& refData, CLR_UINT8* data, CLR_UINT32 size, CLR_RT_HeapBlock* cls, CLR_UINT32* unknownType, CLR_UINT32 flags )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

//--//

HRESULT CLR_RT_BinaryFormatter::TrackDuplicate( CLR_RT_HeapBlock* object )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

CLR_UINT32 CLR_RT_BinaryFormatter::SearchDuplicate( CLR_RT_HeapBlock* object )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    return (CLR_UINT32)-1;
}

CLR_RT_HeapBlock* CLR_RT_BinaryFormatter::GetDuplicate( CLR_UINT32 idx )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    return NULL;
}

//--//--//

int     CLR_RT_BinaryFormatter::BitsAvailable(                                  ) { return 0; }

HRESULT CLR_RT_BinaryFormatter::ReadBits     (       CLR_UINT32& res, int bits  ) { TINYCLR_FEATURE_STUB_RETURN(); }
HRESULT CLR_RT_BinaryFormatter::WriteBits    (       CLR_UINT32  val, int bits  ) { TINYCLR_FEATURE_STUB_RETURN(); }

HRESULT CLR_RT_BinaryFormatter::ReadBits     (       CLR_UINT64& res, int bits  ) { TINYCLR_FEATURE_STUB_RETURN(); }
HRESULT CLR_RT_BinaryFormatter::WriteBits    (       CLR_UINT64  val, int bits  ) { TINYCLR_FEATURE_STUB_RETURN(); }

HRESULT CLR_RT_BinaryFormatter::ReadArray    (       CLR_UINT8*  buf, int bytes ) { TINYCLR_FEATURE_STUB_RETURN(); }
HRESULT CLR_RT_BinaryFormatter::WriteArray   ( const CLR_UINT8*  buf, int bytes ) { TINYCLR_FEATURE_STUB_RETURN(); }

HRESULT CLR_RT_BinaryFormatter::ReadCompressedUnsigned( CLR_UINT32& val )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_BinaryFormatter::WriteCompressedUnsigned( CLR_UINT32 val )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

//--//

HRESULT CLR_RT_BinaryFormatter::ReadType( CLR_RT_ReflectionDef_Index& val )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_BinaryFormatter::WriteType( const CLR_RT_ReflectionDef_Index& val )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}

HRESULT CLR_RT_BinaryFormatter::WriteType( CLR_UINT32 hash )
{
    NATIVE_PROFILE_CLR_SERIALIZATION();
    TINYCLR_FEATURE_STUB_RETURN();
}
