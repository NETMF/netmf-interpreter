#include "stdafx.h"


#define DOUBLE_QUOTES TEXT('"')

void ParamParser::PreParseAll()
{
	try
	{
		// we need to have a delimiter . It cannot be null 
		if (!m_delimiter || !m_separator) throw InvalidParameterException();

		size_t beg = 0;
		while( m_strToParse.length() > (beg = ParseNextParam(beg)));
	}
	catch(std::exception&)
	{
		throw InvalidParameterException();
	}
}


size_t ParamParser::ParseNextParam( size_t beg) 
{
	if ( beg >= m_strToParse.length()) return m_strToParse.length();

	size_t idx = beg;

	// exclude leading spaces if any
	while ( idx < m_strToParse.length() && m_strToParse.at(idx) == TEXT(' ')) idx++;

	if (m_strToParse.at(idx) != m_delimiter)
	{
		throw InvalidParameterException();
	}

	// find the next occurrence of the delimiter. if we don't find any, this is the last param
	// first skip any escaped delimiters 
	size_t endpos = m_strToParse.find(m_delimiter, ++idx);
	while( (endpos != StrType::npos) &&
			(endpos < m_strToParse.length()-1) && 
			(m_strToParse[endpos+1] == m_delimiter))
		{
			endpos = m_strToParse.find(m_delimiter, endpos+2);
		}

	if (endpos == StrType::npos)
	{
		endpos = m_strToParse.length(); 
	}
	
	StrType thisParam = m_strToParse.substr(idx, endpos - idx);
	
	// find the next occurrence of the separator 
	size_t valPos = thisParam.find(m_separator);
	
	if (valPos == StrType::npos)
	{
		valPos = thisParam.length();
	}
	
	StrType param = RightTrim(thisParam.substr(0, valPos));
	StrType value;
	if ( valPos < thisParam.length())
	{
		value =  thisParam.substr(++valPos, thisParam.length() -valPos);
	}
	// remove escape chars 
	//==============
	TCHAR escapeDelim[3] = {m_delimiter, m_delimiter, 0};
	size_t escapePos = value.find(escapeDelim);
	while(escapePos != StrType::npos)
	{
		value.erase(escapePos, 1);
		escapePos = value.find(escapeDelim);
	}
	
	// handle switch type params
	if ((value = RightTrim(value)).empty())
	{
		value = toString<BOOL>(TRUE);
	}
	
	if (param.empty())
	{
		throw InvalidParameterException();
	}

	if ( m_params.find(param) != m_params.end())
	{
		throw InvalidParameterException();
	}
	else
	{
		m_params.insert(std::make_pair(param,value));
	} 

	return endpos;
}


ParamParser::ParamParser( const TCHAR* pCmdLine, TCHAR delimiter, TCHAR separator) 
		: m_delimiter(delimiter),
		  m_separator(separator)
{
	if (NULL == pCmdLine || MAX_LENGTH < _tcslen(pCmdLine))
	{
		throw InvalidParameterException();
	};

	m_strToParse = pCmdLine;

	PreParseAll();

}

size_t ParamParser::countParams() 
{
	return m_params.size();
}


ParamParser::ParamParser( int count, const TCHAR* args[], TCHAR delimiter,  TCHAR separator)
		: m_delimiter(delimiter), 
		 m_separator(separator)
{

	if (NULL == args) 
	{
		throw InvalidParameterException();
	}
	
	m_strToParse = TEXT("");
	
	for ( int i = 1; i < count; i++)
	{
		m_strToParse += StrType(args[i]) + TEXT(' ');
	}
	
	if(MAX_LENGTH < m_strToParse.length())
	{
		throw InvalidParameterException();			
	}
	
	PreParseAll();	
}


StrType ParamParser::GetString(LPCTSTR name, LPCTSTR defaultVal)
{
	// look for the param name
	std::map<StrType, StrType>::iterator pos = m_params.find(name);
	if ( pos == m_params.end())
	{
		return defaultVal;
	} else
	{
		return pos->second;
	}

}

double ParamParser::GetDouble(LPCTSTR name, double defaultVal)
{
	// look for the param name
	std::map<StrType, StrType>::iterator pos = m_params.find(name);
	
	// lookup the param name
	if (pos != m_params.end())
	{
		// if data parsing fails this function will return deafult value
		ON_ERROR_CONTINUE( InvalidParameterException, 
							return ParseData<double>(pos->second.c_str()));

	}
	return defaultVal;

}

DWORD ParamParser::GetDWORD(LPCTSTR name, DWORD defaultVal)
{

	// look for the param name
	std::map<StrType, StrType>::iterator pos = m_params.find(name);
	
	// lookup the param name
	if (pos != m_params.end())
	{
		// if data parsing fails this function will return the default value
		ON_ERROR_CONTINUE( InvalidParameterException, 
			return ParseData<DWORD>(pos->second.c_str()));

	}
	
	return defaultVal;
}

BOOL ParamParser::GetFlag(LPCTSTR name, BOOL defaultVal)
{
	// look for the param name
	std::map<StrType, StrType>::iterator pos = m_params.find(name);

	if (pos != m_params.end())
	{
		// if data parsing fails just return the default value
		ON_ERROR_CONTINUE( InvalidParameterException, 
							return ParseData<BOOL>(pos->second.c_str()));
	}
	
	return defaultVal;
}

int ParamParser::GetInteger(LPCTSTR name, int defaultVal)
{
	// look for the param name
	std::map<StrType, StrType>::iterator pos = m_params.find(name);
	
	if (pos != m_params.end())
	{
		// if data parsing fails just return the default value
		ON_ERROR_CONTINUE( InvalidParameterException, 
							return ParseData<int>(pos->second.c_str()));

	}
	
	return defaultVal;		
}

__int64 ParamParser::GetInt64(LPCTSTR name, __int64 defaultVal)
{
	// look for the param name
	std::map<StrType, StrType>::iterator pos = m_params.find(name);

	if (pos != m_params.end())
	{
		// if data parsing fails just return the default value
		ON_ERROR_CONTINUE( InvalidParameterException, 		
							return ParseData<__int64>(pos->second.c_str()));
	}
	
	return defaultVal;
}



