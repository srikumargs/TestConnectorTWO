#include "StdAfx.h"
#include "ServiceUtils.h"

void DeleteWindowsService(std::ofstream_t& oLogStream, LPCTSTR lpServiceName)
{
	SC_HANDLE schSCManager = NULL;
	SC_HANDLE schService = NULL;

	// Get a handle to the SCM database.  
	schSCManager = OpenSCManager(NULL, NULL, SC_MANAGER_ALL_ACCESS);
	__try
	{
		if(NULL == schSCManager) 
		{
			oLogStream << _T("OpenSCManager failed; result is: ") << GetLastError() << _T(".") << std::endl;
			return;
		}

		// Get a handle to the service.
		schService = OpenService(schSCManager, lpServiceName, DELETE | SERVICE_STOP | SERVICE_QUERY_STATUS | SERVICE_ENUMERATE_DEPENDENTS);
		__try
		{
			if(schService == NULL)
			{ 
				oLogStream << _T("OpenService '") << lpServiceName << _T("' failed; result is: ") << GetLastError() << _T(".") << std::endl;
				return;
			}

			// Stop the service.
			StopWindowsService(oLogStream, schSCManager, schService);

			// Delete the service.
			if(!DeleteService(schService)) 
			{
				oLogStream << _T("DeleteService '") << lpServiceName << _T("' failed; result is: ") << GetLastError() << _T(".") << std::endl;
			}
			else
			{
				oLogStream << _T("DeleteService '") << lpServiceName << _T("' succeeded.")  << std::endl;
			}
		}
		__finally
		{
			if(schService != NULL)
			{
				CloseServiceHandle(schService); 
				schService = NULL;
			}
		}
	}
	__finally
	{
		if(schSCManager != NULL)
		{
			CloseServiceHandle(schSCManager);
			schSCManager = NULL;
		}
	}
}

void StopWindowsService(std::ofstream_t& oLogStream, SC_HANDLE schSCManager, SC_HANDLE schService)
{
	SERVICE_STATUS_PROCESS ssp;
	DWORD dwStartTime = GetTickCount();
	DWORD dwBytesNeeded;
	DWORD dwTimeout = 30000; // 30-second time-out

	// Make sure the service is not already stopped.
	if(!QueryServiceStatusEx(schService, SC_STATUS_PROCESS_INFO, (LPBYTE)&ssp, sizeof(SERVICE_STATUS_PROCESS), &dwBytesNeeded))
	{
		oLogStream << _T("QueryServiceStatusEx failed; result is: ") << GetLastError() << _T(".") << std::endl;
		return;
	}

	if(ssp.dwCurrentState == SERVICE_STOPPED)
	{
		oLogStream << _T("Service is already stopped.") << std::endl;
		return;
	}

	// If a stop is pending, wait for it.
	while(ssp.dwCurrentState == SERVICE_STOP_PENDING) 
	{
		oLogStream << _T("Service stop pending...") << std::endl;
		Sleep(ssp.dwWaitHint);
		if(!QueryServiceStatusEx(schService, SC_STATUS_PROCESS_INFO, (LPBYTE)&ssp, sizeof(SERVICE_STATUS_PROCESS), &dwBytesNeeded))
		{
			oLogStream << _T("QueryServiceStatusEx failed; result is: ") << GetLastError() << _T(".") << std::endl;
			return;
		}

		if(ssp.dwCurrentState == SERVICE_STOPPED)
		{
			oLogStream << _T("Service stopped successfully.") << std::endl;
			return;
		}

		if(GetTickCount() - dwStartTime > dwTimeout)
		{
			oLogStream << _T("Service stop timed out.") << std::endl;
			return;
		}
	}

	// If the service is running, dependencies must be stopped first.
	StopDependentWindowsServices(oLogStream, schSCManager, schService);

	// Send a stop code to the service.
	if(!ControlService(schService, SERVICE_CONTROL_STOP, (LPSERVICE_STATUS) &ssp))
	{
		oLogStream << _T("ControlService failed; result is: ") << GetLastError() << _T(".") << std::endl;
		return;
	}

	// Wait for the service to stop.
	while(ssp.dwCurrentState != SERVICE_STOPPED) 
	{
		Sleep(ssp.dwWaitHint);
		if(!QueryServiceStatusEx(schService, SC_STATUS_PROCESS_INFO, (LPBYTE)&ssp, sizeof(SERVICE_STATUS_PROCESS), &dwBytesNeeded))
		{
			oLogStream << _T("QueryServiceStatusEx failed; result is: ") << GetLastError() << _T(".") << std::endl;
			return;
		}

		if(ssp.dwCurrentState == SERVICE_STOPPED)
		{
			break;
		}

		if(GetTickCount() - dwStartTime > dwTimeout)
		{
			oLogStream << _T("Wait timed out.") << std::endl;
			return;
		}
	}

	oLogStream << _T("Service stopped successfully.") << std::endl;
}

BOOL StopDependentWindowsServices(std::ofstream_t& oLogStream, SC_HANDLE schSCManager, SC_HANDLE schService)
{
	DWORD i;
	DWORD dwBytesNeeded;
	DWORD dwCount;

	LPENUM_SERVICE_STATUS   lpDependencies = NULL;
	ENUM_SERVICE_STATUS     ess;
	SC_HANDLE               hDepService;


	// Pass a zero-length buffer to get the required buffer size.
	if(EnumDependentServices( schService, SERVICE_ACTIVE, lpDependencies, 0, &dwBytesNeeded, &dwCount)) 
	{
		// If the Enum call succeeds, then there are no dependent
		// services, so do nothing.
		return TRUE;
	} 
	else 
	{
		if(GetLastError() != ERROR_MORE_DATA)
		{
			return FALSE; // Unexpected error
		}

		// Allocate a buffer for the dependencies.
		lpDependencies = (LPENUM_SERVICE_STATUS) HeapAlloc(GetProcessHeap(), HEAP_ZERO_MEMORY, dwBytesNeeded);
		__try 
		{
			if(!lpDependencies)
			{
				return FALSE;
			}

			// Enumerate the dependencies.
			if(!EnumDependentServices(schService, SERVICE_ACTIVE, lpDependencies, dwBytesNeeded, &dwBytesNeeded, &dwCount))
			{
				return FALSE;
			}

			for(i = 0; i < dwCount; i++) 
			{
				ess = *(lpDependencies + i);

				// Open the service.
				hDepService = OpenService(schSCManager, ess.lpServiceName, SERVICE_STOP | SERVICE_QUERY_STATUS | SERVICE_ENUMERATE_DEPENDENTS);
				if(!hDepService)
				{
					return FALSE;
				}

				__try
				{
					StopWindowsService(oLogStream, schSCManager, hDepService);
				} 
				__finally 
				{
					// Always release the service handle.
					CloseServiceHandle(hDepService);
				}
			}
		} 
		__finally 
		{
			// Always free the enumeration buffer.
			HeapFree(GetProcessHeap(), 0, lpDependencies);
		}
	} 

	return TRUE;
}

void StopWindowsService(std::ofstream_t& oLogStream, LPCTSTR lpServiceName)
{
	SC_HANDLE schSCManager = NULL;
	SC_HANDLE schService = NULL;

	// Get a handle to the SCM database. 
	schSCManager = OpenSCManager(NULL, NULL, SC_MANAGER_ALL_ACCESS);
	__try
	{
		if(NULL == schSCManager) 
		{
			oLogStream << _T("OpenSCManager failed; result is: ") << GetLastError() << _T(".") << std::endl;
			return;
		}

		// Get a handle to the service.
		schService = OpenService(schSCManager, lpServiceName, DELETE | SERVICE_STOP | SERVICE_QUERY_STATUS | SERVICE_ENUMERATE_DEPENDENTS);
		__try
		{
			if(schService == NULL)
			{ 
				oLogStream << _T("OpenService '") << lpServiceName << _T("' failed; result is: ") << GetLastError() << _T(".") << std::endl;
				return;
			}

			// Stop the service.
			StopWindowsService(oLogStream, schSCManager, schService);
		}
		__finally
		{
			if(schService != NULL)
			{
				CloseServiceHandle(schService); 
				schService = NULL;
			}
		}
	}
	__finally
	{
		if(schSCManager != NULL)
		{
			CloseServiceHandle(schSCManager);
			schSCManager = NULL;
		}
	}
}

void StartWindowsService(std::ofstream_t& oLogStream, LPCTSTR lpServiceName)
{
	SC_HANDLE schSCManager = NULL;
	SC_HANDLE schService = NULL;

	// Get a handle to the SCM database. 
	schSCManager = OpenSCManager(NULL, NULL, SC_MANAGER_ALL_ACCESS);
	__try
	{
		if(NULL == schSCManager) 
		{
			oLogStream << _T("OpenSCManager failed; result is: ") << GetLastError() << _T(".") << std::endl;
			return;
		}

		// Get a handle to the service.
		schService = OpenService(schSCManager, lpServiceName, SERVICE_START | SERVICE_QUERY_STATUS);
		__try
		{
			if(schService == NULL)
			{ 
				oLogStream << _T("OpenService '") << lpServiceName << _T("' failed; result is: ") << GetLastError() << _T(".") << std::endl;
				return;
			}

			// Start the service.
			StartWindowsService(oLogStream, schService);
		}
		__finally
		{
			if(schService != NULL)
			{
				CloseServiceHandle(schService); 
				schService = NULL;
			}
		}
	}
	__finally
	{
		if(schSCManager != NULL)
		{
			CloseServiceHandle(schSCManager);
			schSCManager = NULL;
		}
	}
}

void StartWindowsService(std::ofstream_t& oLogStream, SC_HANDLE schService)
{
	SERVICE_STATUS_PROCESS ssp;
	DWORD dwStartTime = GetTickCount();
	DWORD dwBytesNeeded;
	DWORD dwTimeout = 30000; // 30-second time-out

	// Make sure the service is not already stopped.
	if(!QueryServiceStatusEx(schService, SC_STATUS_PROCESS_INFO, (LPBYTE)&ssp, sizeof(SERVICE_STATUS_PROCESS), &dwBytesNeeded))
	{
		oLogStream << _T("QueryServiceStatusEx failed; result is: ") << GetLastError() << _T(".") << std::endl;
		return;
	}

	if(ssp.dwCurrentState == SERVICE_RUNNING)
	{
		oLogStream << _T("Service is already running.") << std::endl;
		return;
	}

	// If a stop is pending, wait for it.
	while(ssp.dwCurrentState == SERVICE_START_PENDING) 
	{
		oLogStream << _T("Service start pending...") << std::endl;
		Sleep(ssp.dwWaitHint);
		if(!QueryServiceStatusEx(schService, SC_STATUS_PROCESS_INFO, (LPBYTE)&ssp, sizeof(SERVICE_STATUS_PROCESS), &dwBytesNeeded))
		{
			oLogStream << _T("QueryServiceStatusEx failed; result is: ") << GetLastError() << _T(".") << std::endl;
			return;
		}

		if(ssp.dwCurrentState == SERVICE_RUNNING)
		{
			oLogStream << _T("Service running.") << std::endl;
			return;
		}

		if(GetTickCount() - dwStartTime > dwTimeout)
		{
			oLogStream << _T("Service stop timed out.") << std::endl;
			return;
		}
	}

	// Send a stop code to the service.
	if(!StartService(schService, 0, NULL))
	{
		oLogStream << _T("StartService failed; result is: ") << GetLastError() << _T(".") << std::endl;
		return;
	}

	// Wait for the service to stop.
	while(ssp.dwCurrentState != SERVICE_RUNNING) 
	{
		Sleep(ssp.dwWaitHint);
		if(!QueryServiceStatusEx(schService, SC_STATUS_PROCESS_INFO, (LPBYTE)&ssp, sizeof(SERVICE_STATUS_PROCESS), &dwBytesNeeded))
		{
			oLogStream << _T("QueryServiceStatusEx failed; result is: ") << GetLastError() << _T(".") << std::endl;
			return;
		}

		if(ssp.dwCurrentState == SERVICE_RUNNING)
		{
			break;
		}

		if(GetTickCount() - dwStartTime > dwTimeout)
		{
			oLogStream << _T("Wait timed out.") << std::endl;
			return;
		}
	}

	oLogStream << _T("Service started successfully.") << std::endl;
}

BOOL WaitForServiceMutexToBeSet(std::ofstream_t& oLogStream, LPCTSTR lpServiceMutexName, DWORD dwStartTickCount, DWORD dwTimeoutThreshold)
{
	BOOL bResult = FALSE;
	HANDLE hMutex = NULL;

	const DWORD dwElapsed = GetTickCount() - dwStartTickCount;
	if(dwElapsed > dwTimeoutThreshold)
	{
		oLogStream << _T("dwElapsed (") << dwElapsed << _T(") > dwTimeoutThreshold (") << dwTimeoutThreshold << _T(") during wait for '") << lpServiceMutexName <<  _T("'.") << std::endl;
		bResult = TRUE;
	}
	else
	{
		hMutex = OpenMutex(MUTEX_ALL_ACCESS, FALSE, lpServiceMutexName);
		if(hMutex != NULL)
		{
			oLogStream << _T("OpenMutex('") << lpServiceMutexName  << _T("') succeeded; the service process has set the mutex") << std::endl;
			::CloseHandle(hMutex);
			bResult = TRUE;
		}
		else
		{
			oLogStream << _T("OpenMutex('") << lpServiceMutexName  << _T("') failed; result is: ") << GetLastError() << _T(".") << std::endl;
		}
	}

	return bResult;
}

BOOL WaitForServiceMutexToBeReleased(std::ofstream_t& oLogStream, LPCTSTR lpServiceMutexName, DWORD dwStartTickCount, DWORD dwTimeoutThreshold)
{
	BOOL bResult = FALSE;
	HANDLE hMutex = NULL;

	const DWORD dwElapsed = GetTickCount() - dwStartTickCount;
	if(dwElapsed > dwTimeoutThreshold)
	{
		oLogStream << _T("dwElapsed (") << dwElapsed << _T(") > dwTimeoutThreshold (") << dwTimeoutThreshold << _T(") during wait for '") << lpServiceMutexName <<  _T("'.") << std::endl;
		bResult = TRUE;
	}
	else
	{
		hMutex = OpenMutex(MUTEX_ALL_ACCESS, FALSE, lpServiceMutexName);
		if(hMutex == NULL)
		{
			DWORD dwLastError = GetLastError();
			oLogStream << _T("OpenMutex('") << lpServiceMutexName  << _T("') failed; result is: ") << dwLastError << _T(".") << std::endl;
			if(dwLastError == ERROR_FILE_NOT_FOUND)
			{
				oLogStream << _T("OpenMutex('") << lpServiceMutexName  << _T("') failed with ERROR_FILE_NOT_FOUND; the service process has released the mutex.") << std::endl;
				bResult = TRUE;
			}
		}
		else
		{
			oLogStream << _T("OpenMutex('") << lpServiceMutexName  << _T("') succeeded; the service process has not yet released the mutex.") << std::endl;
			::CloseHandle(hMutex);
		}
	}

	return bResult;
}

BOOL ServiceExists(std::ofstream_t& oLogStream, LPCTSTR lpServiceName)
{
	BOOL bResult = FALSE;

	SC_HANDLE schSCManager = NULL;
	SC_HANDLE schService = NULL;

	// Get a handle to the SCM database.  
	schSCManager = ::OpenSCManager(NULL, NULL, SC_MANAGER_ALL_ACCESS);
	__try
	{
		if(NULL == schSCManager) 
		{
			oLogStream << _T("OpenSCManager failed; result is: ") << GetLastError() << _T(".") << std::endl;
			return FALSE;
		}

		// Get a handle to the service.
		schService = ::OpenService(schSCManager, lpServiceName, SERVICE_QUERY_CONFIG | SERVICE_QUERY_STATUS | SERVICE_ENUMERATE_DEPENDENTS);
		__try
 		{
			if(schService != NULL)
			{ 
				bResult = TRUE;
				oLogStream << _T("OpenService '") << lpServiceName << _T("' succeeded; the service exists.") << std::endl;
			}
			else
			{
				oLogStream << _T("OpenService '") << lpServiceName << _T("' failed; result is: ") << GetLastError() << _T(".") << std::endl;
			}
		}
		__finally
		{
			if(schService != NULL)
			{
				::CloseServiceHandle(schService); 
				schService = NULL;
			}
		}
	}
	__finally
	{
		if(schSCManager != NULL)
		{
			::CloseServiceHandle(schSCManager);
			schSCManager = NULL;
		}
	}

	return bResult;
}

BOOL GetServiceStartName(std::ofstream_t& oLogStream, LPCTSTR lpServiceName, std::string_t& sServiceStartName)
{
	sServiceStartName = _T("");
	BOOL bResult = FALSE;

	SC_HANDLE schSCManager = NULL;
	SC_HANDLE schService = NULL;

	// Get a handle to the SCM database.  
	schSCManager = ::OpenSCManager(NULL, NULL, SC_MANAGER_ALL_ACCESS);
	__try
	{
		if(NULL == schSCManager) 
		{
			oLogStream << _T("OpenSCManager failed; result is: ") << GetLastError() << _T(".") << std::endl;
			return FALSE;
		}

		// Get a handle to the service.
		schService = ::OpenService(schSCManager, lpServiceName, SERVICE_QUERY_CONFIG);
		LPQUERY_SERVICE_CONFIG lpsc = NULL; 
		__try
 		{
			DWORD dwBytesNeeded, cbBufSize, dwError; 

			if(schService != NULL)
			{ 
				if(!QueryServiceConfig(
					schService, 
					NULL, 
					0, 
					&dwBytesNeeded))
				{
					dwError = GetLastError();
					if(ERROR_INSUFFICIENT_BUFFER == dwError)
					{
						cbBufSize = dwBytesNeeded;
						lpsc = (LPQUERY_SERVICE_CONFIG) LocalAlloc(LMEM_FIXED, cbBufSize);


						if(!QueryServiceConfig( 
							schService, 
							lpsc, 
							cbBufSize, 
							&dwBytesNeeded)) 
						{
							oLogStream << _T("QueryServiceConfig '") << lpServiceName << _T("' failed; result is: ") << GetLastError() << _T(".") << std::endl;
						}
						else
						{
							bResult = TRUE;
							oLogStream << _T("ChangeServiceConfig '") << lpServiceName << _T("' succeeded.") << std::endl;
							sServiceStartName = lpsc->lpServiceStartName;
						}
					}
					else
					{
						oLogStream << _T("QueryServiceConfig '") << lpServiceName << _T("' failed; result is: ") << dwError << _T(".") << std::endl;
					}
				}
			}
			else
			{
				oLogStream << _T("OpenService '") << lpServiceName << _T("' failed; result is: ") << GetLastError() << _T(".") << std::endl;
			}
		}
		__finally
		{
			if(schService != NULL)
			{
				::CloseServiceHandle(schService); 
				schService = NULL;
			}

			if(lpsc != NULL)
			{
				LocalFree(lpsc); 
				lpsc = NULL;
			}
		}
	}
	__finally
	{
		if(schSCManager != NULL)
		{
			::CloseServiceHandle(schSCManager);
			schSCManager = NULL;
		}
	}

	return bResult;
}