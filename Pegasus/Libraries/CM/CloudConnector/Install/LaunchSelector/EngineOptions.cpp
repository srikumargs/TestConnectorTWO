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
	const DWORD PATH_SIZE = 4096;
	TCHAR tszTempName[PATH_SIZE];
	TCHAR tszPath[PATH_SIZE];

	// Get the temp path
	GetTempPath(PATH_SIZE, tszPath);

	// Create a temporary file.
	GetTempFileName(tszPath, _T("LS-"), 0, tszTempName);

	m_oLogStream.open(tszTempName);
	m_oLogStream << _T("Command line: ") << lpCmdLine << std::endl;
	::GetCurrentDirectory(PATH_SIZE, tszPath);
	m_oLogStream << _T("Working directory: ") << tszPath << std::endl;

	TCHAR tszExePath[PATH_SIZE] = { 0 };
	GetModuleFileName(NULL, tszExePath, PATH_SIZE);
	m_oLogStream << _T("tszExePath: ") << tszExePath << std::endl;
	PathRemoveFileSpec(tszExePath);
	m_sExePath = tszExePath;
}

std::string_t CEngineOptions::GetUpgradeBrowserCommand() const
{
	TCHAR tszTemp[MAX_PATH];
	::PathCombine(tszTemp, m_sExePath.c_str(), _T("SageUpgrade.exe"));
	return std::string_t(tszTemp);
}

std::string_t CEngineOptions::GetInstallBrowserCommand() const
{
	TCHAR tszTemp[MAX_PATH];
	::PathCombine(tszTemp, m_sExePath.c_str(), _T("SageBrowser.exe"));
	return std::string_t(tszTemp);
}

std::string_t CEngineOptions::GetCurrentDirectory() const
{
	return m_sCurrentDirectory;
}