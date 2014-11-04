#include "StdAfx.h"
#include "Exceptions.h"

CExceptionBase::CExceptionBase(const std::string_t& sMethodName, const std::string_t& sMessage)
{
	m_sMessage = sMessage;
	m_sMethodName = sMethodName;
}

CUnresolvedVariablesException::CUnresolvedVariablesException(const std::string_t& sMethodName, const std::string_t& sExpression) :
CExceptionBase(sMethodName, _T("One or more unresolved variable identifiers remain in '%s'."))
{
	m_sExpression = sExpression;
}

CInvalidCommandLineParameterException::CInvalidCommandLineParameterException(const std::string_t& sMethodName, const std::string_t& sParameter, const std::string_t& sExpression) :
CExceptionBase(sMethodName, _T("The '%s' command-line parameter in '%s' is not valid"))
{
	m_sParameter = sParameter;
	m_sExpression = sExpression;
}

CStringArgumentNullOrEmptyException::CStringArgumentNullOrEmptyException(const std::string_t& sMethodName, const std::string_t& sName) :
CExceptionBase(sMethodName, _T("The '%s' parameter of the '%s' method cannot be null or empty.")),
m_sName(sName)
{
}

