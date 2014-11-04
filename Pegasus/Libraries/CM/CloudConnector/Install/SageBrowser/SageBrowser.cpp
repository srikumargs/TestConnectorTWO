#include "stdafx.h"
#include "SageBrowser.h"
#include "BrowserWindow.h"
#include "Configuration.h"
#include "..\LinkedSource\Exceptions.h"

int APIENTRY _tWinMain(HINSTANCE hInstance,
                     HINSTANCE hPrevInstance,
                     LPTSTR    lpCmdLine,
                     int       nCmdShow)
{
	UNREFERENCED_PARAMETER(hPrevInstance);
	UNREFERENCED_PARAMETER(nCmdShow);

	std::ofstream_t ofs;
	int nExitCode = 0;
	try
	{
		CConfiguration config(lpCmdLine, ofs);

		CBrowserWindow browserWindow(hInstance, config);
		browserWindow.Show();
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
