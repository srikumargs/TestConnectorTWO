// LaunchSelector.cpp : Defines the entry point for the application.
//

#include "stdafx.h"
#include "LaunchSelector.h"
#include "EngineOptions.h"
#include "Engine.h"
#include <fstream>
#include "..\LinkedSource\Exceptions.h"

int APIENTRY _tWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPTSTR lpCmdLine, int nCmdShow)
{
	UNREFERENCED_PARAMETER(hInstance);
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(nCmdShow);

	std::ofstream_t ofs;
	int nExitCode = 0;
	try
	{
		CEngineOptions options(lpCmdLine, ofs);
		CEngine engine;
		nExitCode=engine.Execute(options);
	}
	catch(const CExceptionBase& ex)
	{
		ofs << ex.GetMessage();
	}
	catch(...)
	{
		ofs << "Unhandled exception occurred.";
	}

	ofs.close();

	return nExitCode;
}