#include "StdAfx.h"
#include "StringUtils.h"

#include <errno.h>
 

std::vector<std::string_t> Tokenize(const std::string_t& sValue, const std::string_t& sDelims)
{
	// Skip delims at beginning, find start of first token
	std::string_t::size_type lastPos = sValue.find_first_not_of(sDelims, 0); 

	// Find next delimiter @ end of token 
	std::string_t::size_type pos = sValue.find_first_of(sDelims, lastPos);  

	// output vector 
	std::vector<std::string_t> tokens; 
	while (std::string_t::npos != pos || std::string_t::npos != lastPos)  
	{      
		// Found a token, add it to the vector. 
		tokens.push_back(sValue.substr(lastPos, pos - lastPos));  

		// Skip delims.  Note the "not_of". this is beginning of token  
		lastPos = sValue.find_first_not_of(sDelims, pos);   

		// Find next delimiter at end of token. 
		pos = sValue.find_first_of(sDelims, lastPos); 
	} 

	return tokens;
}

std::string_t ToLower(const std::string_t& sValue)
{  
	std::string_t sResult = sValue;

	const short nSize = (short) sResult.size();
	if(0 != nSize)
	{
		for(short i = 0 ; i < nSize ; i++)
		{
			sResult[i] = _totlower(sResult[i]);
		}
	}

	return sResult;
}

std::string_t TrimSpaces(const std::string_t& sValue)
{   
	return Trim(sValue, std::string_t(_T(" \t")));
}   

std::string_t Trim(const std::string_t& sValue, const std::string_t& sDelims)
{
	std::string_t sResult = sValue;
	sResult = TrimStart(sResult, sDelims);
	sResult = TrimEnd(sResult, sDelims);
	return sResult;
}

std::string_t TrimStart(const std::string_t& sValue, const std::string_t& sDelims)
{
	std::string_t sResult = sValue;

	// Code for  Trim Leading Spaces only  
	std::size_t nPos = sResult.find_first_not_of(sDelims);
	if( std::string::npos != nPos )
	{
		sResult = sResult.substr( nPos );
	}
	return sResult;
}

std::string_t TrimEnd(const std::string_t& sValue, const std::string_t& sDelims)
{
	std::string_t sResult = sValue;

	// Code for Trim trailing Spaces only  
	std::size_t nPos = sValue.find_last_not_of(sDelims);
	if( std::string::npos != nPos )  
	{
		sResult = sResult.substr( 0, nPos+1 );  
	}
	return sResult;
}

std::string_t Replace(const std::string_t& sValue, const std::string_t& sFrom, const std::string_t& sTo)
{
	std::string_t sResult = sValue;

	if(sFrom != sTo && !sFrom.empty())
	{
		std::string::size_type nPos1(0);
		std::string::size_type nPos2(0);
		const std::string::size_type nFromSize(sFrom.size());
		const std::string::size_type nToSize(sTo.size());
		while((nPos1 = sResult.find(sFrom, nPos2)) != std::string_t::npos)
		{
			sResult.replace(nPos1, nFromSize, sTo);
			nPos2 = nPos1 + nToSize;
		}
	}

	return sResult;
}

bool StartsWith(const std::string_t& str, const std::string_t& value)
{
	bool bResult = false;

	if(str.substr(0, value.length()) == value)
	{
		bResult = true;
	}

	return bResult;
}

bool EndsWith(const std::string_t &str, const std::string_t& value)
{
	bool bResult = false;
	std::string_t::size_type keylen = value.length();
	std::string_t::size_type strlen = str.length();
		
	if(keylen <= strlen)
		bResult = ((strlen - keylen) == str.rfind(value.c_str(), strlen - keylen, keylen));	

	return bResult;
}


std::string_t format_arg_list(const TCHAR* fmt, va_list args)
{
	if (!fmt) return _T(""); 
	int   result = STRUNCATE, length = 256;   
	TCHAR* buffer = 0;    
	while (result == STRUNCATE)    
	{ 
		if (buffer) delete [] buffer;   
		buffer = new TCHAR[length + 1];  
		memset(buffer, 0, length + 1);  
		result = _vsntprintf_s(buffer, length + 1, _TRUNCATE, fmt, args);     
		length *= 2;  
	}    

	std::string_t s(buffer);  
	delete [] buffer; 
	return s;
}

std::string_t Format(const TCHAR* fmt, ...)
{  
	va_list args;
	va_start(args, fmt);  
	std::string_t s = format_arg_list(fmt, args); 
	va_end(args);    
	return s;
}

DWORD StringToDWORD(const std::string_t& str, int base)
{
	return _tcstol(str.c_str(), NULL, base);
}
