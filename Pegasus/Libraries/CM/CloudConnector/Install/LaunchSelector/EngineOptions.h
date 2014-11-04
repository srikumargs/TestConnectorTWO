#pragma once

#include "..\LinkedSource\VariablesHandler.h"
#include "..\LinkedSource\StringUtils.h"
#include <map>
#include <fstream>

class CEngineOptions
{
public:
	CEngineOptions(LPTSTR lpCmdLine, std::ofstream_t& oLogStream );

	std::string_t GetCurrentDirectory() const;
	std::ofstream_t& GetLogStream() const
	{ return m_oLogStream; }						 

	std::string_t GetUpgradeBrowserCommand() const;
	std::string_t GetInstallBrowserCommand() const;

private:
	std::string_t m_sCurrentDirectory;
	std::string_t m_sExePath;

	std::ofstream_t& m_oLogStream;

	// disallow assignment
	CEngineOptions& operator=(const CEngineOptions&);
};
