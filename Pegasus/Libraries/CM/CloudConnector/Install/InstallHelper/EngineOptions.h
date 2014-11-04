#pragma once

#include "..\LinkedSource\StringUtils.h"
#include "..\LinkedSource\VariablesHandler.h"
#include <list>
#include <map>
#include <fstream>

typedef enum
{
	eNone = 0,
	eUninstall,
	eCloseAll
} Mode;

class CEngineOptions
{
public:
	CEngineOptions(LPTSTR lpCmdLine, std::ofstream_t& oLogStream );

	std::string_t GetArgsOnly() const;
	std::string_t GetCurrentDirectory() const;
	std::ofstream_t& GetLogStream() const
	{ return m_oLogStream; }
	Mode GetMode() const
	{return m_eMode;}
	std::string_t GetInstallPath() const
	{return m_sInstallPath;}
	std::string_t GetVersion() const
	{return m_sVersion;}

private:
	void ModeOptionHandler(const std::string_t& sParams);
	void InstallPathOptionHandler(const std::string_t& sParams);
	void VersionOptionHandler(const std::string_t& sParams);

	Mode m_eMode;
	std::string_t m_sInstallPath;
	std::string_t m_sVersion;

	std::string_t m_sCurrentDirectory;
	std::string_t m_sExePath;

	CVariablesHandler m_variablesHandler;

	typedef std::map<std::string_t, void(CEngineOptions::*)(const std::string_t&), std::less<std::string_t> > OptionHandlerMap;
	OptionHandlerMap m_optionHandlers;

	std::ofstream_t& m_oLogStream;

	// disallow assignment
	CEngineOptions& operator=(const CEngineOptions&);
};