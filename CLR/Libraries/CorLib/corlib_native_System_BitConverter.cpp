////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#include "CorLib.h"

char DigitalToHex(BYTE x)
{
	return x < 10 ? x + '0' :  x - 10 + 'A';
}

char* ByteArrayToHex(BYTE* pInput, int index, int length)
{
	char* pOutput = new char[length * 3];
	char* p = pOutput;

	pInput += index;
	for(int i = 0; i < length; i++, pInput++)
	{
		*p++ = DigitalToHex(*pInput / 16);
		*p++ = DigitalToHex(*pInput % 16);
		*p++ = '-';
	}
	*(--p) = 0;

	return pOutput;
}

HRESULT Library_corlib_native_System_BitConverter::get_IsLittleEndian___STATIC__BOOLEAN( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	DWORD x = 0x12345678;
	BYTE* p = reinterpret_cast<BYTE*>(&x);
	stack.SetResult_Boolean(*p == 0x78);

	TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_BitConverter::DoubleToInt64Bits___STATIC__I8__R8( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
	double input = stack.Arg0().NumericByRefConst().r8;
#else
	CLR_INT64 input = stack.Arg0().NumericByRefConst().r8;
#endif
	__int64* p = reinterpret_cast<__int64*>(&input);
	stack.SetResult_I8(*p);

	TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_BitConverter::GetBytes___STATIC__SZARRAY_U1__BOOLEAN( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	bool input = stack.Arg0().NumericByRefConst().u1 != 0;

	TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(stack.PushValueAndClear(), 1, g_CLR_RT_WellKnownTypes.m_UInt8));
	{
		BYTE* p = stack.TopValue().DereferenceArray()->GetFirstElement();
		*reinterpret_cast<bool*>(p) = input;
	}
	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::GetBytes___STATIC__SZARRAY_U1__CHAR( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	wchar_t input = stack.Arg0().NumericByRefConst().u2;

	TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(stack.PushValueAndClear(), 2, g_CLR_RT_WellKnownTypes.m_UInt8));
	{
		BYTE* p = stack.TopValue().DereferenceArray()->GetFirstElement();
		*reinterpret_cast<wchar_t*>(p) = input;
	}
	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::GetBytes___STATIC__SZARRAY_U1__R8( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
	double input = stack.Arg0().NumericByRefConst().r8;
#else
	CLR_INT64 input = stack.Arg0().NumericByRefConst().r8;
#endif

	TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(stack.PushValueAndClear(), 8, g_CLR_RT_WellKnownTypes.m_UInt8));
	{
		BYTE* p = stack.TopValue().DereferenceArray()->GetFirstElement();
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
		*reinterpret_cast<double*>(p) = input;
#else
		*reinterpret_cast<CLR_INT64*>(p) = input;
#endif
	}
	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::GetBytes___STATIC__SZARRAY_U1__R4( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
	float input = stack.Arg0().NumericByRefConst().r4;
#else
	CLR_INT32 input = stack.Arg0().NumericByRefConst().r4;
#endif

	TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(stack.PushValueAndClear(), 4, g_CLR_RT_WellKnownTypes.m_UInt8));
	{
		BYTE* p = stack.TopValue().DereferenceArray()->GetFirstElement();
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
		*reinterpret_cast<float*>(p) = input;
#else
		*reinterpret_cast<CLR_INT32*>(p) = input;
#endif
	}
	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::GetBytes___STATIC__SZARRAY_U1__I4( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	int input = stack.Arg0().NumericByRefConst().s4;

	TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(stack.PushValueAndClear(), 4, g_CLR_RT_WellKnownTypes.m_UInt8));
	{
		BYTE* p = stack.TopValue().DereferenceArray()->GetFirstElement();
		*reinterpret_cast<int*>(p) = input;
	}
	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::GetBytes___STATIC__SZARRAY_U1__I8( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	__int64 input = stack.Arg0().NumericByRefConst().s8;

	TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(stack.PushValueAndClear(), 8, g_CLR_RT_WellKnownTypes.m_UInt8));
	{
		BYTE* p = stack.TopValue().DereferenceArray()->GetFirstElement();
		*reinterpret_cast<__int64*>(p) = input;
	}
	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::GetBytes___STATIC__SZARRAY_U1__I2( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	short input = stack.Arg0().NumericByRefConst().s2;

	TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(stack.PushValueAndClear(), 2, g_CLR_RT_WellKnownTypes.m_UInt8));
	{
		BYTE* p = stack.TopValue().DereferenceArray()->GetFirstElement();
		*reinterpret_cast<short*>(p) = input;
	}
	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::GetBytes___STATIC__SZARRAY_U1__U4( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	unsigned int input = stack.Arg0().NumericByRefConst().u4;

	TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(stack.PushValueAndClear(), 4, g_CLR_RT_WellKnownTypes.m_UInt8));
	{
		BYTE* p = stack.TopValue().DereferenceArray()->GetFirstElement();
		*reinterpret_cast<unsigned int*>(p) = input;
	}
	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::GetBytes___STATIC__SZARRAY_U1__U8( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	unsigned __int64 input = stack.Arg0().NumericByRefConst().u8;

	TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(stack.PushValueAndClear(), 8, g_CLR_RT_WellKnownTypes.m_UInt8));
	{
		BYTE* p = stack.TopValue().DereferenceArray()->GetFirstElement();
		*reinterpret_cast<unsigned __int64*>(p) = input;
	}
	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::GetBytes___STATIC__SZARRAY_U1__U2( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	unsigned short input = stack.Arg0().NumericByRefConst().u2;

	TINYCLR_CHECK_HRESULT(CLR_RT_HeapBlock_Array::CreateInstance(stack.PushValueAndClear(), 2, g_CLR_RT_WellKnownTypes.m_UInt8));
	{
		BYTE* p = stack.TopValue().DereferenceArray()->GetFirstElement();
		*reinterpret_cast<unsigned short*>(p) = input;
	}
	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::Int64BitsToDouble___STATIC__R8__I8( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	__int64 input = stack.Arg0().NumericByRefConst().s8;
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
	double* p = reinterpret_cast<double*>(&input);
#else
	CLR_INT64* p = reinterpret_cast<CLR_INT64*>(&input);
#endif
	stack.SetResult_R8(*p);

	TINYCLR_NOCLEANUP_NOLABEL();
}

HRESULT Library_corlib_native_System_BitConverter::ToBoolean___STATIC__BOOLEAN__SZARRAY_U1__I4( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	BYTE *p = NULL;
	int index = 0;

	CLR_RT_HeapBlock_Array* pArray = stack.Arg0().DereferenceArray();
	FAULT_ON_NULL_ARG(pArray);

	index = stack.Arg1().NumericByRefConst().s4;
	if (index < 0 || (unsigned int)index >= pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

	p = pArray->GetFirstElement();
	stack.SetResult_Boolean(*reinterpret_cast<bool*>(p + index));

	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::ToChar___STATIC__CHAR__SZARRAY_U1__I4( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	BYTE *p = NULL;
	int index = 0;

	CLR_RT_HeapBlock_Array* pArray = stack.Arg0().DereferenceArray();
	FAULT_ON_NULL_ARG(pArray);

	index = stack.Arg1().NumericByRefConst().s4;
	if (index < 0 || (unsigned int)index >= pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
	if ((unsigned int)index + 2 > pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

	p = pArray->GetFirstElement();
	stack.SetResult(*reinterpret_cast<wchar_t*>(p + index), DATATYPE_CHAR);

	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::ToDouble___STATIC__R8__SZARRAY_U1__I4( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	BYTE *p = NULL;
	int index = 0;

	CLR_RT_HeapBlock_Array* pArray = stack.Arg0().DereferenceArray();
	FAULT_ON_NULL_ARG(pArray);

	index = stack.Arg1().NumericByRefConst().s4;
	if (index < 0 || (unsigned int)index >= pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
	if ((unsigned int)index + 8 > pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

	p = pArray->GetFirstElement();
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
	stack.SetResult_R8(*reinterpret_cast<double*>(p + index));
#else
	stack.SetResult_R8(*reinterpret_cast<CLR_INT64*>(p + index));
#endif

	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::ToSingle___STATIC__R4__SZARRAY_U1__I4( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	BYTE *p = NULL;
	int index = 0;

	CLR_RT_HeapBlock_Array* pArray = stack.Arg0().DereferenceArray();
	FAULT_ON_NULL_ARG(pArray);

	index = stack.Arg1().NumericByRefConst().s4;
	if (index < 0 || (unsigned int)index >= pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
	if ((unsigned int)index + 4 > pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

	p = pArray->GetFirstElement();
#if !defined(TINYCLR_EMULATED_FLOATINGPOINT)
	stack.SetResult_R4(*reinterpret_cast<float*>(p + index));
#else
	stack.SetResult_R4(*reinterpret_cast<CLR_INT32*>(p + index));
#endif

	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::ToInt16___STATIC__I2__SZARRAY_U1__I4( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	BYTE *p = NULL;
	int index = 0;

	CLR_RT_HeapBlock_Array* pArray = stack.Arg0().DereferenceArray();
	FAULT_ON_NULL_ARG(pArray);

	index = stack.Arg1().NumericByRefConst().s4;
	if (index < 0 || (unsigned int)index >= pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
	if ((unsigned int)index + 2 > pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

	p = pArray->GetFirstElement();
	stack.SetResult(*reinterpret_cast<short*>(p + index), DATATYPE_I2);

	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::ToInt32___STATIC__I4__SZARRAY_U1__I4( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	BYTE *p = NULL;
	int index = 0;

	CLR_RT_HeapBlock_Array* pArray = stack.Arg0().DereferenceArray();
	FAULT_ON_NULL_ARG(pArray);

	index = stack.Arg1().NumericByRefConst().s4;
	if (index < 0 || (unsigned int)index >= pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
	if ((unsigned int)index + 4 > pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

	p = pArray->GetFirstElement();
	stack.SetResult_I4(*reinterpret_cast<int*>(p + index));

	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::ToInt64___STATIC__I8__SZARRAY_U1__I4( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	BYTE *p = NULL;
	int index = 0;

	CLR_RT_HeapBlock_Array* pArray = stack.Arg0().DereferenceArray();
	FAULT_ON_NULL_ARG(pArray);

	index = stack.Arg1().NumericByRefConst().s4;
	if (index < 0 || (unsigned int)index >= pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
	if ((unsigned int)index + 8 > pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

	p = pArray->GetFirstElement();
	stack.SetResult_I8(*reinterpret_cast<__int64*>(p + index));

	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::ToUInt16___STATIC__U2__SZARRAY_U1__I4( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	BYTE *p = NULL;
	int index = 0;

	CLR_RT_HeapBlock_Array* pArray = stack.Arg0().DereferenceArray();
	FAULT_ON_NULL_ARG(pArray);

	index = stack.Arg1().NumericByRefConst().s4;
	if (index < 0 || (unsigned int)index >= pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
	if ((unsigned int)index + 2 > pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

	p = pArray->GetFirstElement();
	stack.SetResult(*reinterpret_cast<unsigned short*>(p + index), DATATYPE_U2);

	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::ToUInt32___STATIC__U4__SZARRAY_U1__I4( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	BYTE *p = NULL;
	int index = 0;

	CLR_RT_HeapBlock_Array* pArray = stack.Arg0().DereferenceArray();
	FAULT_ON_NULL_ARG(pArray);

	index = stack.Arg1().NumericByRefConst().s4;
	if (index < 0 || (unsigned int)index >= pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
	if ((unsigned int)index + 4 > pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

	p = pArray->GetFirstElement();
	stack.SetResult_U4(*reinterpret_cast<unsigned int*>(p + index));

	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::ToUInt64___STATIC__U8__SZARRAY_U1__I4( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	BYTE *p = NULL;
	int index = 0;

	CLR_RT_HeapBlock_Array* pArray = stack.Arg0().DereferenceArray();
	FAULT_ON_NULL_ARG(pArray);

	index = stack.Arg1().NumericByRefConst().s4;
	if (index < 0 || (unsigned int)index >= pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
	if ((unsigned int)index + 8 > pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

	p = pArray->GetFirstElement();
	stack.SetResult_U8(*reinterpret_cast<unsigned __int64*>(p + index));

	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::ToString___STATIC__STRING__SZARRAY_U1( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	CLR_RT_HeapBlock_Array* pArray = stack.Arg0().DereferenceArray();
	FAULT_ON_NULL_ARG(pArray);

	if (pArray->m_numOfElements == 0)
	{
		TINYCLR_CHECK_HRESULT(stack.SetResult_String(""));
	}
	else
	{
		BYTE* p = pArray->GetFirstElement();
		char* pOutput = ByteArrayToHex(p, 0, pArray->m_numOfElements);
		TINYCLR_CHECK_HRESULT(stack.SetResult_String(pOutput));
		delete[] pOutput;
	}

	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::ToString___STATIC__STRING__SZARRAY_U1__I4( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	int index = 0;

	CLR_RT_HeapBlock_Array* pArray = stack.Arg0().DereferenceArray();
	FAULT_ON_NULL_ARG(pArray);

	index = stack.Arg1().NumericByRefConst().s4;
	if (pArray->m_numOfElements == 0 && index == 0)
	{
		TINYCLR_CHECK_HRESULT(stack.SetResult_String(""));
	}
	else
	{
		if (index < 0 || (unsigned int)index >= pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);

		BYTE* p = pArray->GetFirstElement();
		char* pOutput = ByteArrayToHex(p, index, pArray->m_numOfElements - index);
		TINYCLR_CHECK_HRESULT(stack.SetResult_String(pOutput));
		delete[] pOutput;
	}

	TINYCLR_NOCLEANUP();
}

HRESULT Library_corlib_native_System_BitConverter::ToString___STATIC__STRING__SZARRAY_U1__I4__I4( CLR_RT_StackFrame& stack )
{
	NATIVE_PROFILE_CLR_CORE();
	TINYCLR_HEADER();

	int index = 0;
	int length = 0;

	CLR_RT_HeapBlock_Array* pArray = stack.Arg0().DereferenceArray();
	FAULT_ON_NULL_ARG(pArray);

	index = stack.Arg1().NumericByRefConst().s4;
	length = stack.Arg2().NumericByRefConst().s4;
	if (pArray->m_numOfElements == 0 && index == 0 && length == 0)
	{
		TINYCLR_CHECK_HRESULT(stack.SetResult_String(""));
	}
	else
	{
		if (index < 0 || length < 0 || (unsigned int)index >= pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_OUT_OF_RANGE);
		if ((unsigned int)index + length > pArray->m_numOfElements) TINYCLR_SET_AND_LEAVE(CLR_E_INVALID_PARAMETER);

		BYTE* p = pArray->GetFirstElement();
		char* pOutput = ByteArrayToHex(p, index, length);
		TINYCLR_CHECK_HRESULT(stack.SetResult_String(pOutput));
		delete[] pOutput;
	}

	TINYCLR_NOCLEANUP();
}
