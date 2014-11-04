#pragma once

#include "EngineOptions.h"

class CEngine
{
public:
	CEngine();

	DWORD Execute(const CEngineOptions& options);

private:
	void Uninstall(const CEngineOptions& options);
	void CloseAll(const CEngineOptions& options);
};

