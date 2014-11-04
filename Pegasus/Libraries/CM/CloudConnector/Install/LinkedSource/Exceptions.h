#pragma once
#include "..\LinkedSource\StringUtils.h"

class CExceptionBase
{
public:
	CExceptionBase(const std::string_t& sMethodName, const std::string_t& sMessage);

	virtual std::string_t GetMessage() const
	{ return m_sMessage; }

	std::string_t GetMethodName() const
	{ return m_sMethodName; }

private:
	std::string_t m_sMessage;
	std::string_t m_sMethodName;
};

class CUnresolvedVariablesException : public CExceptionBase
{
public:
	CUnresolvedVariablesException(const std::string_t& sMethodName, const std::string_t& sExpression);

	virtual std::string_t GetMessage() const
	{
		return Format(GetMessage().c_str(), GetExpression().c_str());
	}

	std::string_t GetExpression() const
	{ return m_sExpression; }

private:
	std::string_t m_sExpression;
};


class CInvalidCommandLineParameterException : public CExceptionBase
{
public:
	CInvalidCommandLineParameterException(const std::string_t& sMethodName, const std::string_t& sParameter, const std::string_t& sExpression);

	virtual std::string_t GetMessage() const
	{
		return Format(GetMessage().c_str(), GetParameter().c_str(), GetExpression().c_str());
	}

	std::string_t GetParameter() const
	{ return m_sParameter; }

	std::string_t GetExpression() const
	{ return m_sExpression; }

private:
	std::string_t m_sParameter;
	std::string_t m_sExpression;
};

class CStringArgumentNullOrEmptyException : public CExceptionBase
{
public:
	CStringArgumentNullOrEmptyException(const std::string_t& sMethodName, const std::string_t& sName);

	virtual std::string_t GetMessage() const
	{
		return Format(GetMessage().c_str(), GetName().c_str(), GetMethodName().c_str());
	}

	std::string_t GetName() const
	{ return m_sName; }

private:
	std::string_t m_sName;
};

