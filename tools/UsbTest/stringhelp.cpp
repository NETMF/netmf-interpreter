#include "stdafx.h"


BOOL cmpwstri(const TCHAR* left, const  TCHAR* right, LCID locId)
{
	return CompareString(locId, NORM_IGNORECASE, left, -1, right, -1);
};

template<> 
StringType toString< bool >(bool data, const TCHAR* format)
{
	if (data)
	{
		return TEXT("TRUE") ;
	} else
	{
		return TEXT("FALSE");
	}
};

template <>
__int64 ParseData<__int64>(const TCHAR* strData)
{
	__int64 value = 0;
	
	if (strData)
	{
		value = _tstoi64(strData);
	}
	return value;
}


StringType RightTrim(const StringType& str, wchar_t ch)
{
	StringType trimmed = str;
	size_t idx = str.length() -1;

	while ( (idx >= 0) && (str[idx] == ch)) --idx;
	
	if ( idx+1 < str.length())
		trimmed.erase(idx+1,  str.length() -(idx +1));
	
	return trimmed;
}

StringType LeftTrim(const StringType& str, wchar_t ch)
{
	StringType trimmed = str;
	StringType::size_type idx = 0;

	while ( idx < str.length() && str[idx] == ch) idx++;
	if ( idx > 0)
		trimmed.erase(0, idx);
	
	return trimmed;
}

std::vector<StringType> SplitString(StringType str, TCHAR delimiter)
{
	std::vector<StringType> ret;

	size_t beg = 0;
	size_t pos = str.find(delimiter);
	while (pos != StringType::npos)
	{
		ret.push_back( str.substr(beg, pos -beg));
		beg = pos + 1;
		pos = str.find(delimiter, beg);
	}
	ret.push_back(str.substr(beg, str.length() - beg));

	return ret;
}

