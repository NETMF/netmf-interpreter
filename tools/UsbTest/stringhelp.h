#ifndef STRINGHELP_H__
#define STRINGHELP_H__

//**********************************************************************************
// StringHelp : 
//		Miscellaneous String manipulation /conversion /parsing classes
//
//	The classes in this suite include : 
//		ParseData  - used to convert string to number datatype of choice  
//					Eg.: int myAge = ParseData<int>(strAge.c_str());
//
//		toString - used to convert any variable to string. 
//					Eg.: wmi::wstring myBankBalance = toString<int>(nBankBal); or
//						wprintf(L"Boolean Value=%s\n", toString<bool>(true).c_str());
//
//		LeftTrim & RightTrim - Trim out leading /trailing characters like SPACE from a string
//**********************************************************************************

#include <sstream>
#include <windows.h>
#include <vector>


#if defined(UNICODE)
typedef std::wstring       StringType;
typedef std::wstringstream StringStreamType;
#else
typedef std::string       StringType;
typedef std::stringstream StringStreamType;
#endif // UNICODE

typedef std::basic_string<char, std::char_traits<char>,std::allocator<char> > ansi_string;
typedef std::basic_string<wchar_t, std::char_traits<wchar_t>,std::allocator<wchar_t> > wide_string;


BOOL cmpwstri(const wchar_t* left, const  wchar_t* right, LCID locId = LOCALE_INVARIANT);

template <typename T> 
T ParseData( const TCHAR* strData) 
{
	StringStreamType stream(strData);
	
	stream.exceptions( std::ios::failbit |std::ios::badbit);
	T targetData;	
	
	try
	{
		stream >> targetData;
	}
	catch( std::ios::failure&)
	{
		throw InvalidParameterException();
	}
	return targetData;
};

// specialization to implement ParseData for 64-bit datatypes
template <>
__int64 ParseData<__int64>(const TCHAR* strData);


StringType RightTrim(const StringType& str, wchar_t ch = TEXT(' '));

StringType LeftTrim(const StringType& str, wchar_t ch = TEXT(' '));



template<typename T> 
StringType toString(T number, const TCHAR* format = NULL)
{
	if (NULL != format)
	{
		// the maximum 64 bit number would have 20 digits+(.) +(0x /oct)+(20 digits for precision doubles) = ~60 chars
		std::vector<TCHAR> buffer(60 + _tcslen(format));
		_sntprintf(&buffer[0],buffer.size(), format, number);
		
		// ensure null termination
		buffer[buffer.size() -1] = L'\0';
		return &buffer[0];
	} else
	{
		StringStreamType strval;
		strval.exceptions( std::ios::failbit |std::ios::badbit);
		
		strval << number;
		return strval.str().c_str();
	}
};


template<> 
StringType toString< bool >(bool data, const TCHAR* format);


std::vector<StringType> SplitString(StringType str, wchar_t delimiter);


template<typename _strType, typename _charTyp>
_strType RTrim( _strType& str, _charTyp ch)
{
	_strType trimmed = str;
	
	size_t idx = str.length() -1;

	while ( (idx >= 0) && (str[idx] == ch)) --idx;
	
	if ( idx+1 < str.length())
		trimmed.erase(idx+1,  str.length() -(idx +1));
	
	return trimmed;
};


template<typename _strTyp, typename _charTyp>
_strTyp LTrim( _strTyp& str, _charTyp ch)
{
	_strTyp trimmed = str;
	size_t idx = 0;

	while ( idx < str.length() && str[idx] == ch) idx++;
	if ( idx > 0)
		trimmed.erase(0, idx);
	
	return trimmed;
};

ansi_string ConvertString( const wide_string& wstr);

wide_string ConvertString( const ansi_string& str);

#endif
