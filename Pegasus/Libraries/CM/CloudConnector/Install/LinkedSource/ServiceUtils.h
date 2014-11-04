#pragma once

#include "..\LinkedSource\StringUtils.h"

void DeleteWindowsService(std::ofstream_t& oLogStream, LPCTSTR lpServiceName);
void StopWindowsService(std::ofstream_t& oLogStream, SC_HANDLE schSCManager, SC_HANDLE schService);
void StopWindowsService(std::ofstream_t& oLogStream, LPCTSTR lpServiceName);
BOOL StopDependentWindowsServices(std::ofstream_t& oLogStream, SC_HANDLE schSCManager, SC_HANDLE schService);
void StartWindowsService(std::ofstream_t& oLogStream, LPCTSTR lpServiceName);
void StartWindowsService(std::ofstream_t& oLogStream, SC_HANDLE schService);
BOOL WaitForServiceMutexToBeSet(std::ofstream_t& oLogStream, LPCTSTR lpServiceMutexName, DWORD dwStartTickCount, DWORD dwTimeoutThreshold);
BOOL WaitForServiceMutexToBeReleased(std::ofstream_t& oLogStream, LPCTSTR lpServiceMutexName, DWORD dwStartTickCount, DWORD dwTimeoutThreshold);
BOOL ServiceExists(std::ofstream_t& oLogStream, LPCTSTR lpServiceName);
BOOL GetServiceStartName(std::ofstream_t& oLogStream, LPCTSTR lpServiceName, std::string_t& sServiceStartName);