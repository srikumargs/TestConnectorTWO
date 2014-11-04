#pragma once
#include "..\LinkedSource\StringUtils.h"
#include "TextAttributes.h"

enum CommandType
{
	ctNone = 0,
	ctShellExecOpen,
	ctExit,
	ctUpgradeProcess
};

class CDynamicTextInfo
{
public:
	CDynamicTextInfo(DWORD dwX, DWORD dwY, const std::string_t& sText, CommandType eCommandType, const std::string_t& sCommand, const std::string_t& sCommandParameters, const CTextAttributes& oTextAttributes);

	DWORD GetX() const
	{ return m_dwX; }

	DWORD GetY() const
	{ return m_dwY; }

	const std::string_t& GetText() const
	{ return m_sText; }

	CommandType GetCommandType() const
	{ return m_eCommandType; }

	const std::string_t& GetCommand() const
	{ return m_sCommand; }

	const std::string_t& GetCommandParameters() const
	{ return m_sCommandParameters; }

	const CTextAttributes& GetTextAttributes() const
	{ return m_oTextAttributes; }

private:
	DWORD m_dwX;
	DWORD m_dwY;
	std::string_t m_sText;
	CommandType m_eCommandType;
	std::string_t m_sCommand;
	std::string_t m_sCommandParameters;
	CTextAttributes m_oTextAttributes;
};
