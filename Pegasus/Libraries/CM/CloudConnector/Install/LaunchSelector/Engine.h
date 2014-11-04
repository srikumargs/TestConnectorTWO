#pragma once

#include "EngineOptions.h"

class CEngine
{
public:
	CEngine();

	DWORD Execute(const CEngineOptions& options);

private:
	BOOL ExecCmdAndWait(const std::string_t& sCommand, const std::string_t& sCurrentDirectory, DWORD* pdwExitCode);
};
