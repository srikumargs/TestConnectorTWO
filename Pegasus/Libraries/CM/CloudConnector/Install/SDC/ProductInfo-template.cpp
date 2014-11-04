#include "stdafx.h"
#include "ProductInfo.h" 
#include "msi.h"

typedef std::map<std::string_t, std::string_t> StringMap;

BOOL IsProductInstalled(std::ofstream_t& stream, LPCTSTR tszProductCode)
{
	BOOL bResult = FALSE;

	INSTALLSTATE installState = MsiQueryProductState(tszProductCode);
	stream << _T("MsiQueryProductState for '") << tszProductCode << _T("'; result is ") << installState << _T(".") << std::endl;
	if(installState == INSTALLSTATE_DEFAULT || installState == INSTALLSTATE_LOCAL)
	{
		bResult = TRUE;
	}

	return bResult;
}

BOOL IsOldProductInstalled(std::ofstream_t& stream)
{
	BOOL bResult = FALSE;

	StringMap map;
	map[_T("848C141D-D54D-49A2-BAC4-B3A6F751A7F8")] = _T("1.0.50301.7");
	map[_T("3B8341BA-ABF9-473E-A176-A3F674D9EA43")] = _T("1.0.50302.1 (or newer)");
	map[_T("B95D9FB7-DCA9-4D3E-A002-37574374767B")] = _T("1.1.50507.1 (or newer)");
	map[_T("36E5241B-1610-4A0E-A266-2A92C84EDB14")] = _T("1.2.50702.1 (or newer)");
	map[_T("F5D18E23-9997-4DE0-A063-87CE2C3C9FA8")] = _T("1.5.00225.1 (or newer)");		//First time entry version
	map[_T("837F7E18-DCEE-4DA1-B28B-8BD9110DF163")] = _T("2.0.00321.1 (or newer)");	    //First s100 compatibility
	map[_T("58EB1BEB-9978-4B5A-B0D7-41F95D10CA1B")] = _T("2.1.00627.1 (or newer)");     //Blob timeouts to 30 minutes
	map[_T("0A00D9AC-6DC2-4E62-8A93-32B231B8D415")] = _T("3.0 (or newer)");     //Pegasus
	//map[_T("168CF325-99E5-449B-B7A1-4859B8F223A3")] = _T("2.2.00917.1 (or newer)");     //Connector Configuration
	// TurnTheCrank: put new product ID <-> version number mappings here after each release
    // NOTE:  GUID value must be ALL UPPERCASE, and must NOT include the leading/trailing braces: '{' '}'.

	for(StringMap::iterator i = map.begin() ; i != map.end() ; i++)
	{
		std::string_t sProductCode = _T("{") + (*i).first + _T("}");
		if(IsProductInstalled(stream, sProductCode.c_str()))
		{
			stream << _T("-=TAG_FullProductName=- v") << (*i).second << _T(" is installed.") << std::endl;
			bResult = TRUE;
			break;
		}
	}

	return bResult;
}

LPCTSTR GetConnectorHostingFxServiceName()
{ return _T("-=TAG_HostingFxServiceName=-"); }

LPCTSTR GetConnectorHostingFxServiceReadyMutexName()
{ return _T("-=TAG_HostingFxServiceReadyMutexName=-"); }

LPCTSTR GetConnectorHostingFxServiceProcessRunningMutexName()
{ return _T("-=TAG_HostingFxServiceProcessRunningMutexName=-"); }

LPCTSTR GetMonitorHostingFxServiceName()
{ return _T("-=TAG_MonitorHostingFxServiceName=-"); }

LPCTSTR GetMonitorHostingFxServiceReadyMutexName()
{ return _T("-=TAG_MonitorHostingFxServiceReadyMutexName=-"); }

LPCTSTR GetMonitorHostingFxServiceProcessRunningMutexName()
{ return _T("-=TAG_MonitorHostingFxServiceProcessRunningMutexName=-"); }

LPCTSTR GetConnectorServiceInstanceAppDataFolderExpression()
{return _T("$(Environment::Folder::COMMON_APPDATA)\\Sage\\CM\\HostingFramework\\-=TAG_HostingFxServiceName=-");}

LPCTSTR GetCurrentlyInstalledProductInstallPathExpression()
{ return _T("$(Environment::Registry::HKLM\\Software\\Sage\\SageConnector\\-=TAG_ProductId=-\\InstallPath)"); }

LPCTSTR GetCurrentlyInstalledProductVersionExpression()
{ return _T("$(Environment::Registry::HKLM\\Software\\Sage\\SageConnector\\-=TAG_ProductId=-\\ProductVersion)"); }

LPCTSTR GetFullProductName()
{ return _T("-=TAG_FullProductName=-"); }

LPCTSTR GetBriefProductName()
{ return _T("-=TAG_BriefProductName=-"); }

LPCTSTR GetMonitorFullProductName()
{ return _T("-=TAG_MonitorFullProductName=-"); }

LPCTSTR GetMonitorBriefProductName()
{ return _T("-=TAG_MonitorBriefProductName=-"); }

