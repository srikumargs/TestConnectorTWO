#ifdef BUILDING_PRODUCTINFO
#else
	#if _DEBUG
		#pragma comment(lib, "ProductInfo_d.lib")
	#else
		#pragma comment(lib, "ProductInfo.lib")
	#endif
#endif

#include "..\LinkedSource\StringUtils.h"
#include <fstream>

#ifdef __cplusplus
extern "C" {
#endif

	extern BOOL IsOldProductInstalled(std::ofstream_t& stream);
	extern LPCTSTR GetConnectorHostingFxServiceName();
	extern LPCTSTR GetConnectorHostingFxServiceReadyMutexName();
	extern LPCTSTR GetConnectorHostingFxServiceProcessRunningMutexName();
	extern LPCTSTR GetMonitorHostingFxServiceName();
	extern LPCTSTR GetMonitorHostingFxServiceReadyMutexName();
	extern LPCTSTR GetMonitorHostingFxServiceProcessRunningMutexName();
	extern LPCTSTR GetConnectorServiceInstanceAppDataFolderExpression();
	extern LPCTSTR GetCurrentlyInstalledProductInstallPathExpression();
	extern LPCTSTR GetCurrentlyInstalledProductVersionExpression();
	extern LPCTSTR GetFullProductName();
	extern LPCTSTR GetBriefProductName();
	extern LPCTSTR GetMonitorFullProductName();
	extern LPCTSTR GetMonitorBriefProductName();

#ifdef __cplusplus
}
#endif