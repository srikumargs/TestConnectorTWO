#include "StdAfx.h"
#include "EngineOptions.h"
#include <functional>
#include <algorithm>
#include <vector>
#include <iterator>
#include <regex>
#include <shlobj.h>
#include <Shlwapi.h>
#include "..\LinkedSource\Exceptions.h"

CEngineOptions::CEngineOptions(LPTSTR lpCmdLine, std::ofstream_t& oLogStream) : m_oLogStream(oLogStream)
{
	TCHAR tszTempName[MAX_PATH];
	TCHAR tszPath[MAX_PATH];

	// Get the temp path
	GetTempPath(MAX_PATH, tszPath);

	// Create a temporary file.
	GetTempFileName(tszPath, _T("SI-"), 0, tszTempName);

	m_oLogStream.open(tszTempName);
	m_oLogStream << _T("Command line: ") << lpCmdLine << std::endl;
	::GetCurrentDirectory(MAX_PATH, tszPath);
	m_oLogStream << _T("Working directory: ") << tszPath << std::endl;

	TCHAR tszExePath[MAX_PATH] = { 0 };
	GetModuleFileName(NULL, tszExePath, MAX_PATH);
	m_oLogStream << _T("tszExePath: ") << tszExePath << std::endl;
	PathRemoveFileSpec(tszExePath);
	m_sExePath = tszExePath;

	// add mappings for all options
	m_optionHandlers.insert(std::make_pair(std::string_t(_T("mode")), &CEngineOptions::ModeOptionHandler));
	m_optionHandlers.insert(std::make_pair(std::string_t(_T("installpath")), &CEngineOptions::InstallPathOptionHandler));
	m_optionHandlers.insert(std::make_pair(std::string_t(_T("version")), &CEngineOptions::VersionOptionHandler));

	// parse the command line (and optional option params) and invoke option handlers
	StringVector sCommandLine = Tokenize(std::string_t(lpCmdLine), _T("/"));
	for(StringVector::size_type i = 0;i < sCommandLine.size(); i++)
	{
		std::string_t sOption = TrimSpaces(sCommandLine[i]);
		std::string_t sParams;
		std::string_t::size_type nPos = sOption.find_first_of(_T(":"));
		if(nPos != std::string_t::npos)
		{
			sParams = Trim(TrimSpaces(sOption.substr(nPos + 1)), _T("\""));
			sOption = sOption.substr(0, nPos);
		}

		// invoke the handler for this option
		OptionHandlerMap::iterator iterFind = m_optionHandlers.find(sOption);
		if(iterFind != m_optionHandlers.end())
		{
			((*this).*(iterFind->second))(sParams);
		}
	}
}

std::string_t CEngineOptions::GetCurrentDirectory() const
{
	return m_sCurrentDirectory;
}

void CEngineOptions::ModeOptionHandler(const std::string_t& sParams)
{
	if(sParams.empty())
	{
		throw CInvalidCommandLineParameterException(_T("CEngineOptions::ModeOptionHandler"), _T("mode"), sParams);
	}

	if(sParams == _T("uninstall"))
	{
		m_eMode = eUninstall;
	}
	else if(sParams == _T("closeall"))
	{
		m_eMode = eCloseAll;
	}
}

void CEngineOptions::InstallPathOptionHandler(const std::string_t& sParams)
{
	if(sParams.empty())
	{
		throw CInvalidCommandLineParameterException(_T("CEngineOptions::ModeOptionHandler"), _T("installpath"), sParams);
	}

	m_sInstallPath = sParams;
}

void CEngineOptions::VersionOptionHandler(const std::string_t& sParams)
{
	if(sParams.empty())
	{
		throw CInvalidCommandLineParameterException(_T("CEngineOptions::ModeOptionHandler"), _T("version"), sParams);
	}

	m_sVersion = sParams;
}
