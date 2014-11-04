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
#include "IniFile.h"

CEngineOptions::CEngineOptions(LPTSTR lpCmdLine, std::ofstream_t& oLogStream) : m_oLogStream(oLogStream),
m_bIsShellExecCommand(false),
m_bCopyToTemp(false)
{
	const DWORD PATH_SIZE = 4096;
	TCHAR tszTempName[PATH_SIZE];
	TCHAR tszPath[PATH_SIZE];

	// Get the temp path
	GetTempPath(PATH_SIZE, tszPath);

	// Create a temporary file.
	GetTempFileName(tszPath, _T("SL-"), 0, tszTempName);

	m_oLogStream.open(tszTempName);
	m_oLogStream << _T("Command line: ") << lpCmdLine << std::endl;
	::GetCurrentDirectory(PATH_SIZE, tszPath);
	m_oLogStream << _T("Working directory: ") << tszPath << std::endl;

	TCHAR tszExePath[PATH_SIZE] = { 0 };
	GetModuleFileName(NULL, tszExePath, PATH_SIZE);
	m_oLogStream << _T("tszExePath: ") << tszExePath << std::endl;
	PathRemoveFileSpec(tszExePath);
	m_sExePath = tszExePath;

	// if no command-line was specified, then attempt to load from a .ini
	std::string_t sCmdLine(lpCmdLine); 
	if(sCmdLine.length() == 0)
	{
		TCHAR tszModuleFilename[PATH_SIZE];
		::GetModuleFileName(NULL, tszModuleFilename, PATH_SIZE);
		::PathRemoveExtension(tszModuleFilename);
		::PathCombine(tszPath, m_sExePath.c_str(), tszModuleFilename);
		std::string_t sIniFilePath = tszPath;
		sIniFilePath += _T(".ini");

		m_oLogStream << "Loading configuration '" << sIniFilePath << "'" << std::endl;
		CIniFile oIniFile(sIniFilePath.c_str());
		oIniFile.Load();

		m_oLogStream << "Reading General section from configuration" << std::endl;
		sCmdLine = oIniFile.ReadString(_T("General"), _T("CommandLine"), _T(""));
	}

	// add mappings for all options
	m_optionHandlers.insert(std::make_pair(std::string_t(_T("execute")), &CEngineOptions::ExecuteOptionHandler));
	m_optionHandlers.insert(std::make_pair(std::string_t(_T("open")), &CEngineOptions::OpenOptionHandler));
	m_optionHandlers.insert(std::make_pair(std::string_t(_T("args")), &CEngineOptions::ArgsOptionHandler));
	m_optionHandlers.insert(std::make_pair(std::string_t(_T("currentDirectory")), &CEngineOptions::CurrentDirectoryOptionHandler));
	m_optionHandlers.insert(std::make_pair(std::string_t(_T("exitCodes")), &CEngineOptions::ExitCodesOptionHandler));
	m_optionHandlers.insert(std::make_pair(std::string_t(_T("copyToTemp")), &CEngineOptions::CopyToTempOptionHandler));
	m_optionHandlers.insert(std::make_pair(std::string_t(_T("elevate")), &CEngineOptions::ElevateOptionHandler));

	// parse the command line (and optional option params) and invoke option handlers
	StringVector sCommandLine = Tokenize(sCmdLine, _T("/"));
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

std::string_t CEngineOptions::GetCommandOnly() const
{
	return m_sExecuteFileName;
}

std::string_t CEngineOptions::GetArgsOnly() const
{
	return m_sArgs;
}

std::string_t CEngineOptions::GetCommand() const
{
	std::ostringstream_t os;
	os << m_sExecuteFileName;
	if(!m_sArgs.empty())
	{
		os << _T(" ") << m_sArgs;
	}	
	return os.str();
}

DWORD CEngineOptions::GetMappedExitCode(DWORD dwExitCode) const
{
	DWORD dwResult = dwExitCode;

	ExitCodesMap::const_iterator iterFind = m_exitCodes.find(dwExitCode);
	if(iterFind != m_exitCodes.end())
	{
		dwResult = (*iterFind).second;
	}
	else if(m_bDefaultExitCodeSpecified)
	{
		dwResult = m_dwDefaultExitCode;
	}

	return dwResult;
}

void CEngineOptions::ExecuteOptionHandler(const std::string_t& sParams)
{
	m_bIsShellExecCommand = false;
	if(sParams.empty())
	{
		throw CInvalidCommandLineParameterException(_T("CEngineOptions::ExecuteOptionHandler"), _T("execute"), sParams);
	}

	m_sExecuteFileName = m_variablesHandler.ExpandAllVariables(sParams);
	if(::PathIsRelative(m_sExecuteFileName.c_str()))
	{
		TCHAR tszTemp[MAX_PATH];
		::PathCombine(tszTemp, m_sExePath.c_str(), m_sExecuteFileName.c_str());
		m_sExecuteFileName = tszTemp;
	}
}

void CEngineOptions::OpenOptionHandler(const std::string_t& sParams)
{
	m_bIsShellExecCommand = true;
	if(sParams.empty())
	{
		throw CInvalidCommandLineParameterException(_T("CEngineOptions::OpenOptionHandler"), _T("open"), sParams);
	}

	m_sExecuteFileName = m_variablesHandler.ExpandAllVariables(sParams);
	if(::PathIsRelative(m_sExecuteFileName.c_str()))
	{
		TCHAR tszTemp[MAX_PATH];
		::PathCombine(tszTemp, m_sExePath.c_str(), m_sExecuteFileName.c_str());
		m_sExecuteFileName = tszTemp;
	}
}

void CEngineOptions::ArgsOptionHandler(const std::string_t& sParams)
{
	if(sParams.empty())
	{
		throw CInvalidCommandLineParameterException(_T("CEngineOptions::ArgsOptionHandler"), _T("args"), sParams);
	}

	m_sArgs = m_variablesHandler.ExpandAllVariables(sParams);
}

void CEngineOptions::CurrentDirectoryOptionHandler(const std::string_t& sParams)
{
	if(sParams.empty())
	{
		throw CInvalidCommandLineParameterException(_T("CEngineOptions::CurrentDirectoryOptionHandler"), _T("currentDirectory"), sParams);
	}

	m_sCurrentDirectory = m_variablesHandler.ExpandAllVariables(sParams);
}

void CEngineOptions::ExitCodesOptionHandler(const std::string_t& sParams)
{
	if(sParams.empty())
	{
		throw CInvalidCommandLineParameterException(_T("CEngineOptions::ExitCodesOptionHandler"), _T("exitCodes"), sParams);
	}

	std::string_t::size_type nPos = sParams.find_first_of(_T("="));
	if(nPos == std::string_t::npos)
	{
		throw CInvalidCommandLineParameterException(_T("CEngineOptions::ExitCodesOptionHandler"), _T("exitCodes"), sParams);
	}

	std::string_t sResultExitCode = sParams.substr(0, nPos);
	int nResultExitCode = _ttoi(sResultExitCode.c_str());

	std::string_t sMappedCodes = sParams.substr(nPos + 1);
	if(sMappedCodes.size() < 1)
	{
		throw CInvalidCommandLineParameterException(_T("CEngineOptions::ExitCodesOptionHandler"), _T("exitCodes"), sParams);
	}

	StringVector sCodes = Tokenize(sMappedCodes, _T(", "));
	for(std::string_t::size_type i = 0 ; i < sCodes.size() ; i++)
	{
		if(sCodes[i].compare(_T("*")) == 0)
		{
			m_bDefaultExitCodeSpecified = true;
			m_dwDefaultExitCode = nResultExitCode;
		}
		m_exitCodes[_ttoi(sCodes[i].c_str())] = nResultExitCode;
	}
}

void CEngineOptions::CopyToTempOptionHandler(const std::string_t&)
{
	m_bCopyToTemp = true;
}

void CEngineOptions::ElevateOptionHandler(const std::string_t&)
{
	m_bElevate = true;
}

std::string_t CEngineOptions::GetCurrentDirectory() const
{
	return m_sCurrentDirectory;
}