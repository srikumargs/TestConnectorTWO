#pragma once

#include "..\LinkedSource\VariablesHandler.h"
#include "..\LinkedSource\StringUtils.h"
#include <map>
#include <fstream>

class CEngineOptions
{
public:
	CEngineOptions(LPTSTR lpCmdLine, std::ofstream_t& oLogStream );

	std::string_t GetArgsOnly() const;
	std::string_t GetCurrentDirectory() const;
	std::ofstream_t& GetLogStream() const
	{ return m_oLogStream; }						 

	std::string_t GetCommandOnly() const;
	std::string_t GetCommand() const;
	bool IsShellExecCommand() const
	{ return m_bIsShellExecCommand; }
	DWORD GetMappedExitCode(DWORD dwExitCode) const;
	bool IsCopyToTemp() const
	{return m_bCopyToTemp;}
	bool GetElevate() const
	{return m_bElevate;}

private:
	void ExecuteOptionHandler(const std::string_t& sParams);
	void OpenOptionHandler(const std::string_t& sParams);
	void ArgsOptionHandler(const std::string_t& sParams);
	void CurrentDirectoryOptionHandler(const std::string_t& sParams);
	void ExitCodesOptionHandler(const std::string_t& sParams);
	void CopyToTempOptionHandler(const std::string_t& sParams);
	void ElevateOptionHandler(const std::string_t& sParams);

	std::string_t m_sExecuteFileName;
	std::string_t m_sArgs;
	typedef std::map<DWORD, DWORD> ExitCodesMap;
	ExitCodesMap m_exitCodes;
	bool m_bDefaultExitCodeSpecified;
	DWORD m_dwDefaultExitCode;
	bool m_bCopyToTemp;
	bool m_bIsShellExecCommand;
	bool m_bElevate;

	std::string_t m_sCurrentDirectory;
	std::string_t m_sExePath;

	CVariablesHandler m_variablesHandler;

	typedef std::map<std::string_t, void(CEngineOptions::*)(const std::string_t&), std::less<std::string_t> > OptionHandlerMap;
	OptionHandlerMap m_optionHandlers;

	std::ofstream_t& m_oLogStream;

	// disallow assignment
	CEngineOptions& operator=(const CEngineOptions&);
};
