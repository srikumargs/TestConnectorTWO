#include "StdAfx.h"
#include "Engine.h"
#include <shellapi.h>
#include <Shlwapi.h>
#include "..\ProductInfo\ProductInfo.h"
#include "..\LinkedSource\UACUtils.h"

CEngine::CEngine()
{}

DWORD CEngine::Execute(const CEngineOptions& options)
{
	DWORD dwExitCode = 1;

	if(IsOldProductInstalled(options.GetLogStream()))
	{
		// Must ShellExecute rather than CreateProcess because the UpgradeBrowserCommand requires elevation (requireAdministrator in manifest).
		// The LaunchSelector purposefully does NOT require elevation up front because it _might_ invoke the InstallBrowserCommand (which doesn't 
		// require elevation until the install is actually kicked off).
		SHELLEXECUTEINFO ShExecInfo = {0};
		ShExecInfo.cbSize = sizeof(SHELLEXECUTEINFO);
		ShExecInfo.fMask = SEE_MASK_NOCLOSEPROCESS;
		ShExecInfo.hwnd = NULL;
		if(!IsRunAsAdmin())
		{
			// only runas if we are not already running as an admin ... needed because on XP runas will cause
			// the confusing dialog to come up that includes the Data Execution Prevention option even if the current 
			// user is an administrator ... using this condition prevents use of runas on non-UAC systems when the user
			// already is an admin
			ShExecInfo.lpVerb = _T("runas");
		}
		else
		{
			ShExecInfo.lpVerb = _T("open");
		}
		TCHAR tszFile[MAX_PATH];
		_tcscpy_s(tszFile, options.GetUpgradeBrowserCommand().c_str());
		ShExecInfo.lpFile = tszFile;		
		ShExecInfo.lpParameters = _T("");
		TCHAR tszDir[MAX_PATH];
		_tcscpy_s(tszDir, options.GetCurrentDirectory().c_str());
		ShExecInfo.lpDirectory = tszDir;
		ShExecInfo.nShow = SW_SHOW;
		ShExecInfo.hInstApp = NULL;	
		ShellExecuteEx(&ShExecInfo);
		WaitForSingleObject(ShExecInfo.hProcess,INFINITE);
	}
	else
	{
		ExecCmdAndWait(options.GetInstallBrowserCommand(), options.GetCurrentDirectory(), &dwExitCode);
	}

	return dwExitCode;
}

BOOL CEngine::ExecCmdAndWait(const std::string_t& sCommand, const std::string_t& sCurrentDirectory, DWORD* pdwExitCode)
{
	BOOL bResult = FALSE;
	STARTUPINFO si;
	DWORD dwExitCode;
	SECURITY_ATTRIBUTES saProcess;
	SECURITY_ATTRIBUTES saThread;
	PROCESS_INFORMATION pi;

	ZeroMemory(&si, sizeof(si));
	si.cb = sizeof(si);

	saProcess.nLength = sizeof(saProcess);
	saProcess.lpSecurityDescriptor = NULL;
	saProcess.bInheritHandle = TRUE;

	saThread.nLength = sizeof(saThread);
	saThread.lpSecurityDescriptor = NULL;
	saThread.bInheritHandle = FALSE;

	bResult = CreateProcess(NULL, 
		(LPTSTR) sCommand.c_str(), 
		&saProcess, 
		&saThread, 
		FALSE, 
		DETACHED_PROCESS, 
		NULL, 
		sCurrentDirectory.empty() ? NULL : sCurrentDirectory.c_str(), 
		&si, 
		&pi);

	if (bResult)
	{
		CloseHandle(pi.hThread);
		WaitForSingleObject(pi.hProcess, INFINITE);
		GetExitCodeProcess(pi.hProcess, &dwExitCode);
		CloseHandle(pi.hProcess);

		*pdwExitCode = dwExitCode;
	}

	return bResult;
}