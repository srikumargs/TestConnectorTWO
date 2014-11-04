#include "StdAfx.h"
#include "DynamicTextInfo.h"

CDynamicTextInfo::CDynamicTextInfo(DWORD dwX, DWORD dwY, const std::string_t& sText, CommandType eCommandType, const std::string_t& sCommand, const std::string_t& sCommandParameters, const CTextAttributes& oTextAttributes) :
	m_dwX(dwX),
	m_dwY(dwY),
	m_sText(sText),
	m_eCommandType(eCommandType),
	m_sCommand(sCommand),
	m_sCommandParameters(sCommandParameters),
	m_oTextAttributes(oTextAttributes)
{
}
