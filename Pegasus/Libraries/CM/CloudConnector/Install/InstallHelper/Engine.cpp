#include "StdAfx.h"
#include "Engine.h"
#include <shlobj.h>
#include <shlwapi.h>
#include <Psapi.h>
#include "../LinkedSource/ServiceUtils.h"
#include "../LinkedSource/FileUtils.h"
#include "../ProductInfo/ProductInfo.h"
#include "../LinkedSource/RunningProcessManager.h"
#include "../LinkedSource/FirewallUtils.h"

CEngine::CEngine()
{}

DWORD CEngine::Execute(const CEngineOptions& options)
{
	options.GetLogStream() << _T("Mode is: ") << options.GetMode() << _T(".") << std::endl;
	Mode mode = options.GetMode();
	if(mode == eUninstall)
	{
		Uninstall(options);
	}
	else if(mode == eCloseAll)
	{
		CloseAll(options);
	}

	return 0;
}

void CEngine::CloseAll(const CEngineOptions& options)
{
	// stop any running processes
	CVariablesHandler variablesHandler;
	std::string_t sDir = options.GetInstallPath();
	if(sDir.length() > 0)
	{
		CRunningProcessManager mgr;
		mgr.StopRunningProcesses(options.GetLogStream(), sDir);
	}
	else
	{
		options.GetLogStream() << _T("Skipping stopping of running processes because install path expression did not yield a value") <<std::endl;
	}
}


bool PathIsExcludedFromCopyOperation(const std::string_t& value)
{ return (EndsWith(value, _T("\\Documents")) || EndsWith(value, _T("\\Backups")) || EndsWith(value, _T("\\Baseline"))); }


void CEngine::Uninstall(const CEngineOptions& options)
{
	TCHAR tszCommonAppDataPath[MAX_PATH + 1];

	::SHGetSpecialFolderPath(NULL, tszCommonAppDataPath, CSIDL_COMMON_APPDATA, FALSE);

	// 1) delete Hosting Framework services
	DeleteWindowsService(options.GetLogStream(), GetMonitorHostingFxServiceName());
	DeleteWindowsService(options.GetLogStream(), GetConnectorHostingFxServiceName());


	// 2) stop running processes
	CloseAll(options);


	// 3) backup data and delete
	CVariablesHandler variablesHandler;
	std::string_t sDir = options.GetInstallPath();
	std::string_t sDatabasePathDir = variablesHandler.ExpandAllVariables(GetConnectorServiceInstanceAppDataFolderExpression());
	if(sDatabasePathDir.length() >0)
	{
		std::string_t sUninstallBackupsDatabasePathDir = sDatabasePathDir;
		sUninstallBackupsDatabasePathDir += _T("\\Backups\\");

		TCHAR tszDateTimeStamp[MAX_PATH];
		::SecureZeroMemory(tszDateTimeStamp, sizeof(tszDateTimeStamp));
		time_t lt;
		time (&lt);
		tm mytm;
		localtime_s(&mytm, &lt); // get current time
		_tcsftime(tszDateTimeStamp, MAX_PATH, _T("%Y%m%d.%H%M%S"), &mytm); // format date

		sUninstallBackupsDatabasePathDir += tszDateTimeStamp;
		sUninstallBackupsDatabasePathDir += _T("_");
		sUninstallBackupsDatabasePathDir += options.GetVersion();
		sUninstallBackupsDatabasePathDir += _T("_Uninstall");

		std::list<std::string_t> fileNames = FindFiles(options.GetLogStream(), (sDatabasePathDir).c_str(), _T("*.*"));
		fileNames.remove_if(PathIsExcludedFromCopyOperation);
		CopyFiles(options.GetLogStream(), fileNames, sUninstallBackupsDatabasePathDir);
		DeleteFiles(options.GetLogStream(), fileNames);

		TCHAR tszDocumentsPathDir[MAX_PATH + 1];
		::PathCombine(tszDocumentsPathDir, sDatabasePathDir.c_str(), _T("Documents"));
		DeleteDirectoryRecursive(options.GetLogStream(), tszDocumentsPathDir);
	}


	// 4) cleanup application exception (Windows XP Firewall)
	if(sDir.length() > 0)
	{
		std::string_t sImagePath = sDir + _T("Monitor\\Service\\Sage.CRE.HostingFramework.Service.exe");
		CleanupFirewallApplicationException(options.GetLogStream(), sImagePath);
	}
	else
	{
		options.GetLogStream() << _T("Skipping removal of firewall application exception because install path expression did not yield a value") <<std::endl;
	}

	if(sDir.length() > 0)
	{
		std::string_t sImagePath = sDir + _T("Sage.CRE.HostingFramework.Service.exe");
		CleanupFirewallApplicationException(options.GetLogStream(), sImagePath);
	}
	else
	{
		options.GetLogStream() << _T("Skipping removal of firewall application exception because install path expression did not yield a value") <<std::endl;
	}

	// 5) Cleanup firewall rule (Windows Firewall with Advanced Security)
	CleanupAdvancedFirewallRule(options.GetLogStream(), GetFullProductName());
	CleanupAdvancedFirewallRule(options.GetLogStream(), GetMonitorFullProductName());
}