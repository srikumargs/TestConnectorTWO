#include "StdAfx.h"
#include "Engine.h"
#include <shellapi.h>
#include <Shlwapi.h>
#include "..\LinkedSource\UACUtils.h"

CEngine::CEngine()
{
}

DWORD CEngine::Execute(const CEngineOptions& options)
{
	DWORD dwExitCode = 1;
	if(options.IsShellExecCommand())
	{
		dwExitCode = 0;
		options.GetLogStream() << _T("Performing 'ShellExecute open' of '") << options.GetCommand() << _T("' in directory '") << options.GetCurrentDirectory() << _T("'.") << std::endl;

		if(options.GetElevate() && !IsRunAsAdmin())
		{
			// only runas if we are not already running as an admin ... needed because on XP runas will cause
			// the confusing dialog to come up that includes the Data Execution Prevention option even if the current 
			// user is an administrator ... using this condition prevents use of runas on non-UAC systems when the user
			// already is an admin

			if (((UINT)::ShellExecute(NULL, _T("runas"), options.GetCommand().c_str(), NULL, NULL, SW_SHOWNORMAL)) <= 32)
			{
				MessageBeep(0);
				dwExitCode = 1;
			}
		}
		else
		{
			if (((UINT)::ShellExecute(NULL, _T("open"), options.GetCommand().c_str(), NULL, NULL, SW_SHOWNORMAL)) <= 32)
			{
				MessageBeep(0);
				dwExitCode = 1;
			}
		}
	}
	else
	{
		options.GetLogStream() << _T("Performing 'create process' of '") << options.GetCommand() << _T("' in directory '") << options.GetCurrentDirectory() << _T("'.") << std::endl;

		std::string_t sWorkingTempDir;
		std::string_t sCommand = options.GetCommand();
		if(options.IsCopyToTemp())
		{
			dwExitCode = DoCopyToTempProcessing(options, sCommand, sWorkingTempDir);
		}

		if(ExecCmdAndWait(sCommand, options.GetCurrentDirectory(), &dwExitCode))
		{
			dwExitCode = options.GetMappedExitCode(dwExitCode);
		}

		if(options.IsCopyToTemp() && sWorkingTempDir.length() > 0)
		{
			DeleteDirectory(options, sWorkingTempDir);
		}
	}

	return dwExitCode;
}



void CEngine::DeleteDirectory(const CEngineOptions& options, const std::string_t& sDirectory)
{
	WIN32_FIND_DATA FindFileData;
	HANDLE hFind = INVALID_HANDLE_VALUE;
	TCHAR tszSourceDir[MAX_PATH];
	::PathCombine(tszSourceDir, sDirectory.c_str(), _T("*"));
	hFind = FindFirstFile(tszSourceDir, &FindFileData);
	if (hFind == INVALID_HANDLE_VALUE) 
	{
		options.GetLogStream() << _T("Invalid file handle; error is ") << GetLastError() << std::endl;
	} 
	else 
	{
		TCHAR tszSourceFile[MAX_PATH];
		::PathCombine(tszSourceFile, sDirectory.c_str(), FindFileData.cFileName);
		if(!::DeleteFile(tszSourceFile))
		{
			options.GetLogStream() << _T("Failed to delete '") << tszSourceFile << _T("'; error is ") << GetLastError() << std::endl;
		}

		while (FindNextFile(hFind, &FindFileData) != 0) 
		{
			::PathCombine(tszSourceFile, sDirectory.c_str(), FindFileData.cFileName);
			if(!::DeleteFile(tszSourceFile))
			{
				options.GetLogStream() << _T("Failed to delete '") << tszSourceFile << _T("'; error is ") << GetLastError() << std::endl;
			}
		}

		DWORD dwError = GetLastError();
		FindClose(hFind);
		if (dwError != ERROR_NO_MORE_FILES) 
		{
			options.GetLogStream() << _T("Invalid file handle; error is ") << GetLastError() << std::endl;
		}
	}


	::RemoveDirectory(sDirectory.c_str());
}

DWORD CEngine::DoCopyToTempProcessing(const CEngineOptions& options, std::string_t& sCommand, std::string_t& sWorkingTempDir)
{
	DWORD result = 0;

	options.GetLogStream() << _T("IsCopyToTemp is true.") << std::endl;

	TCHAR tszRootTempPath[MAX_PATH];
	::GetTempPath(MAX_PATH, tszRootTempPath);

	TCHAR tszWorkingTempDirectory[MAX_PATH];
	::GetTempFileName(tszRootTempPath, _T("SL-"), 0, tszWorkingTempDirectory);
	::DeleteFile(tszWorkingTempDirectory);
	::CreateDirectory(tszWorkingTempDirectory, NULL);
	sWorkingTempDir = tszWorkingTempDirectory;

	TCHAR tszCommand[MAX_PATH];
	::PathCombine(tszCommand, tszWorkingTempDirectory, ::PathFindFileName(options.GetCommandOnly().c_str()));
	sCommand = tszCommand;
	sCommand += _T(" ");
	sCommand += options.GetArgsOnly();

	TCHAR tszFileNameOnly[MAX_PATH];
	_tcscpy_s(tszFileNameOnly, options.GetCommandOnly().c_str());
	::PathRemoveExtension(tszFileNameOnly);
	_tcscat_s(tszFileNameOnly, _T("*"));

	TCHAR tszSourceDirOnly[MAX_PATH];
	_tcscpy_s(tszSourceDirOnly, options.GetCommandOnly().c_str());
	::PathRemoveFileSpec(tszSourceDirOnly);

	WIN32_FIND_DATA FindFileData;
	HANDLE hFind = INVALID_HANDLE_VALUE;
	hFind = FindFirstFile(tszFileNameOnly, &FindFileData);
	if (hFind == INVALID_HANDLE_VALUE) 
	{
		options.GetLogStream() << _T("Invalid file handle; error is ") << GetLastError() << std::endl;
		MessageBeep(0);
		result = 1;
	} 
	else 
	{
		TCHAR tszSourceFile[MAX_PATH];
		::PathCombine(tszSourceFile, tszSourceDirOnly, FindFileData.cFileName);
		if(!CopyFileToDirectory(tszSourceFile, tszWorkingTempDirectory))
		{
			options.GetLogStream() << _T("Failed to copy '") << tszSourceFile << _T("' to '") << tszWorkingTempDirectory << _T("'; error is ") << GetLastError() << std::endl;
			MessageBeep(0);
			result = 1;
		}

		while (FindNextFile(hFind, &FindFileData) != 0) 
		{
			::PathCombine(tszSourceFile, tszSourceDirOnly, FindFileData.cFileName);
			if(!CopyFileToDirectory(tszSourceFile, tszWorkingTempDirectory))
			{
				options.GetLogStream() << _T("Failed to copy '") << tszSourceFile << _T("' to '") << tszWorkingTempDirectory << _T("'; error is ") << GetLastError() << std::endl;
				MessageBeep(0);
				result = 1;
			}
		}

		DWORD dwError = GetLastError();
		FindClose(hFind);
		if (dwError != ERROR_NO_MORE_FILES) 
		{
			options.GetLogStream() << _T("Invalid file handle; error is ") << GetLastError() << std::endl;
			MessageBeep(0);
			result = 1;
		}
	}

	return result;
}

BOOL CEngine::CopyFileToDirectory(const std::string_t& sFile, const std::string_t& sDirectory)
{
	const DWORD PATH_SIZE = 4096;
	TCHAR tszDestPath[PATH_SIZE];
	::PathCombine(tszDestPath, sDirectory.c_str(), ::PathFindFileName(sFile.c_str()));
	return ::CopyFile(sFile.c_str(), tszDestPath, FALSE);
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