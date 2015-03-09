#ifndef __PARAM_PARSER_H__
#define __PARAM_PARSER_H__

#include <comutil.h>
#include <map>
#include <TCHAR.h>
#include "StringHelp.h"


#ifdef UNICODE
	typedef std::wstring StrType;
#else
	typedef std::string StrType;
#endif

#define MAX_LENGTH 256

#define ON_ERROR_CONTINUE(x,y) try{y;}catch(x&){};


class ParamParser
{
	TCHAR m_delimiter;
	TCHAR m_separator;
	StrType m_strToParse; 

	std::map<StrType, StrType> m_params;

	void PreParseAll();

	size_t ParseNextParam(size_t begin);
	
public:
	typedef std::map<StrType, StrType>::iterator iterator;
	
	ParamParser( const TCHAR* pCmdLine,
					TCHAR delimiter = TEXT('/'),
					TCHAR separator = TEXT(' '));

	ParamParser( 	const VARIANT* pvLine,
					TCHAR cDelimiter = TEXT('/'),
					TCHAR separator = TEXT(' '));

	ParamParser( int count,
					const TCHAR* args[],
					TCHAR cDelimiter = TEXT('/'),
					TCHAR separator = TEXT(' '));

	size_t countParams();
	
	ParamParser::iterator begin()
	{
		return m_params.begin();
	}

	ParamParser::iterator end()
	{
		return m_params.end();
	}

	bool Contains(LPCTSTR name)
	{
		return m_params.end() != m_params.find(name);
	}
	
	StrType GetString(LPCTSTR name, LPCTSTR defaultVal = TEXT(""));

	double GetDouble(LPCTSTR name, double defaultVal = 0);

	DWORD GetDWORD(LPCTSTR name, DWORD defaultVal = 0);

	BOOL GetFlag(LPCTSTR name, BOOL defaultVal = FALSE);

	int GetInteger(LPCTSTR name, int defaultVal =0);

	__int64 GetInt64(LPCTSTR name, __int64 defaultVal =0);
};

#endif // __PARAM_PARSER_H__