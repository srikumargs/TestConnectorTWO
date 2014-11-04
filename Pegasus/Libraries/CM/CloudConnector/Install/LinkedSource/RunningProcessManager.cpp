#include "StdAfx.h"
#include "RunningProcessManager.h"

#include <Psapi.h>


void ReportError(std::ofstream_t& oLogStream, LPTSTR szAPI)
{
	DWORD dwLastError = GetLastError();

	LPTSTR MessageBuffer;

	oLogStream << szAPI << _T("()")<<std::endl;

	if(FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM,
		NULL,
		dwLastError,
		GetSystemDefaultLangID(),
		(LPTSTR) &MessageBuffer,
		0,
		NULL))
	{
		oLogStream << MessageBuffer << std::endl;


		// 
		// free the buffer allocated by the system
		// 
		LocalFree(MessageBuffer);
	}
} 

void CRunningProcessManager::StopRunningProcesses(std::ofstream_t& oLogStream, const std::string_t& sDir)
{
	oLogStream << _T("sDir: ")<< sDir << std::endl;

	std::string_t sDirAsLower = sDir;
	Lowercase(sDirAsLower);

	DWORD		aProcesses[1024];
	DWORD		cbNeeded = 0;
	DWORD		cProcesses = 0;
	::SecureZeroMemory(aProcesses, sizeof(aProcesses));

	if(!EnumProcesses( aProcesses, sizeof(aProcesses), &cbNeeded))
	{
		ReportError(oLogStream, _T("EnumProcesses"));
	}

	// Calculate how many process identifiers were returned.
	cProcesses = cbNeeded / sizeof(DWORD);

	// Print the name and process identifier for each process.
	for(unsigned int j = 0; j < cProcesses; j++)
	{
		GetProcessInfo(aProcesses[j]);
	}

	for(ModulesInUseContainer::iterator moduleInUseIter = m_ModulesInUse.begin() ; moduleInUseIter != m_ModulesInUse.end(); moduleInUseIter++)
	{
		if((*moduleInUseIter).first.find(sDirAsLower) == 0)
		{
			RunningProcessesContainer RunningProcesses = (*moduleInUseIter).second;
			for(RunningProcessesContainer::iterator iterRP = RunningProcesses.begin(); RunningProcesses.end() != iterRP ; iterRP++)
			{
				// terminate it as a regular process
				HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, (*iterRP).ID());
				if(!hProcess)
				{
					ReportError(oLogStream, _T("OpenProcess"));
					oLogStream <<  _T("Could not open process for termination: ")<< (*iterRP).Name().c_str() << _T(" (PID# ") << (*iterRP).ID() << _T(")") << std::endl;
					continue;
				}

				if(!TerminateProcess(hProcess, 0xffffffff))
				{
					ReportError(oLogStream, _T("TerminateProcess"));
					CloseHandle(hProcess);
					continue;
				}

				oLogStream << _T("  >> Terminated: ")<<  (*iterRP).FileName().c_str()<< _T(" (PID# ") << (*iterRP).ID() << _T(")")<<std::endl;

				if(!CloseHandle(hProcess))
				{
					ReportError(oLogStream, _T("CloseHandle"));
					continue;
				}
			}
		}
	}
}

void CRunningProcessManager::GetProcessInfo(DWORD dwProcessID)
{
	HMODULE hMods[1024];
	HANDLE hProcess = NULL;
	DWORD cbNeeded = 0;
	::SecureZeroMemory(hMods, sizeof(hMods));


	// Get a list of all the modules in this process.
	hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, dwProcessID);
	if(hProcess != NULL)
	{
		if(EnumProcessModules(hProcess, hMods, sizeof(hMods), &cbNeeded))
		{
			TCHAR szProcessName[MAX_PATH] = _T("unknown");
			GetModuleBaseName(hProcess, hMods[0], szProcessName, sizeof(szProcessName));

			TCHAR szModName[MAX_PATH];
			::SecureZeroMemory(szModName, sizeof(szModName));

			TCHAR szLongPathName[MAX_PATH];
			::SecureZeroMemory(szLongPathName, sizeof(szLongPathName));
			GetModuleFileNameEx(hProcess, hMods[0], szModName, sizeof(szModName));

			if(0 == GetLongPathName(szModName, szLongPathName, MAX_PATH))
			{
				_tcscpy_s(szLongPathName, _countof(szLongPathName), szModName);
			}

			std::string_t sBeforeLowercase = szLongPathName;
			Lowercase(sBeforeLowercase);
			_tcscpy_s(szLongPathName, _countof(szLongPathName), sBeforeLowercase.c_str());



			TCHAR  szTemp1[MAX_PATH];
			::SecureZeroMemory(szTemp1, sizeof(szTemp1));

			TCHAR  szTemp2[MAX_PATH];
			::SecureZeroMemory(szTemp2, sizeof(szTemp2));
			std::string_t sModuleNameNoExtension;
			std::string_t sServiceFileName;

			_tcscpy_s(szTemp1, _countof(szTemp1), szProcessName);
			TCHAR* szFoundLastPeriod = _tcsrchr(szTemp1, '.');
			_tcsncpy_s(szTemp2, _countof(szTemp2), szTemp1, szFoundLastPeriod-szTemp1);
			szTemp2[szFoundLastPeriod-szTemp1] = _T('\0');
			sModuleNameNoExtension = szTemp2;


			CRunningProcess RunningProcess(dwProcessID, sModuleNameNoExtension, szLongPathName);
			for(unsigned int i = 0; i < (cbNeeded / sizeof(HMODULE)); i++)
			{
				// Get the full path to the module's file.
				if(GetModuleFileNameEx(hProcess, hMods[i], szModName, sizeof(szModName)))
				{
					if(0 == GetLongPathName(szModName, szLongPathName, MAX_PATH))
					{
						_tcscpy_s(szLongPathName, _countof(szLongPathName), szModName);
					}

					std::string_t sBeforeLowercase = szLongPathName;
					Lowercase(sBeforeLowercase);
					_tcscpy_s(szLongPathName, _countof(szLongPathName), sBeforeLowercase.c_str());


					ModulesInUseContainer::iterator MIUIter = m_ModulesInUse.find(szLongPathName);
					if(m_ModulesInUse.end() != MIUIter)
					{
						(*MIUIter).second.push_back(RunningProcess);
					}
					else 
					{
						// indexing operation on a map performs an insert
						m_ModulesInUse[szLongPathName];

						m_ModulesInUse[szLongPathName].push_back(RunningProcess);
					}
				}
			}
		}

		CloseHandle(hProcess);
	}
}

void CRunningProcessManager::Lowercase(std::string_t& str)
{
	const unsigned int length = str.length();
	for(unsigned int i = 0 ; i < length ; i++) 
	{
		str[i] = _totlower(str[i]);
	}
}
