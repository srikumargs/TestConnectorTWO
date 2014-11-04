#pragma once

#include "EngineOptions.h"

class CEngine
{
public:
	CEngine();

	DWORD Execute(const CEngineOptions& options);

private:
	BOOL ExecCmdAndWait(const std::string_t& sCommand, const std::string_t& sCurrentDirectory, DWORD* pdwExitCode);
	DWORD DoCopyToTempProcessing(const CEngineOptions& options, std::string_t& sCommand, std::string_t& sWorkingTempDir);
	BOOL CopyFileToDirectory(const std::string_t& sFile, const std::string_t& sDirectory);
	void DeleteDirectory(const CEngineOptions& options, const std::string_t& sDirectory);
};
