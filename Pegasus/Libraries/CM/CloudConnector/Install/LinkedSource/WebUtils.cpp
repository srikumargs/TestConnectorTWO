#include "StdAfx.h"
#include "WebUtils.h"
#include <shlobj.h>
//#include <shlwapi.h>
//#include <initguid.h>
//#include <objbase.h>
//#include <iads.h>
//#include <adshlp.h>
//#include <iiisext.h>
//#include <iisext_i.c>
//#include <atlbase.h>
#include <iads.h>
#include <iiisext.h>
//#include <objbase.h>
//#include <initguid.h>
#include <atlbase.h>
#include <Adshlp.h>

void DeleteWebApp(std::ofstream_t& oLogStream, LPCWSTR lpcwstrAppPath)
{
	HRESULT hr;
	CComPtr<IADs> spADs;
	hr = ADsGetObject(lpcwstrAppPath, IID_IADs, (void**)&spADs);
	if(FAILED(hr))
	{
		oLogStream << _T("ADsGetObject '") << lpcwstrAppPath << _T("' failed; result is: ") << hr << _T(".") << std::endl;
		return;
	}

	CComPtr<IISApp> spApp;
	hr = spADs.QueryInterface(&spApp);
	if(FAILED(hr))
	{
		oLogStream << _T("QI for IISApp on '") << lpcwstrAppPath << _T("' failed; result is: ") << hr << _T(".") << std::endl;
		return;
	}

	hr = spApp->AppDeleteRecursive();
	if(FAILED(hr))
	{
		oLogStream << _T("AppDeleteRecursive for IISApp on '") << lpcwstrAppPath << _T("' failed; result is: ") << hr << _T(".") << std::endl;
		return;
	}

	hr = spApp->SetInfo();
	if(FAILED(hr))
	{
		oLogStream << _T("SetInfo for IISApp on '") << lpcwstrAppPath << _T("' failed; result is: ") << hr << _T(".") << std::endl;
		return;
	} 

	if(SUCCEEDED(hr))
	{
		oLogStream << _T("DeleteWebApp '") << lpcwstrAppPath << _T("' succeeded.") << std::endl;
	}
}

void DeleteVirtualDir(std::ofstream_t& oLogStream, LPCWSTR lpcwstrAppPath)
{
	HRESULT hr;
	CComPtr<IADsContainer> spADsContainer;
	hr = ADsGetObject(L"IIS://localhost/w3svc/1/root", IID_IADsContainer, (void**)&spADsContainer);
	if(FAILED(hr))
	{
		oLogStream << _T("ADsGetObject '") << L"IIS://localhost/w3svc/1/root" << _T("' failed; result is: ") << hr << _T(".") << std::endl;
		return;
	}

	hr = spADsContainer->Delete(CComBSTR(L"IIsWebVirtualDir"), CComBSTR(lpcwstrAppPath));
	if(FAILED(hr))
	{
		oLogStream << _T("Delete IIsWebVirtualDir'") << lpcwstrAppPath << _T("' failed; result is: ") << hr << _T(".") << std::endl;
		return;
	}

	if(SUCCEEDED(hr))
	{
		oLogStream << _T("DeleteVirtualDir '") << lpcwstrAppPath << _T("' succeeded.") << std::endl;
	}
}

void DeleteAppPool(std::ofstream_t& oLogStream, LPCWSTR lpcwstrAppPool)
{
	HRESULT hr;
	CComPtr<IADsContainer> spADsContainer;
	hr = ADsGetObject(L"IIS://localhost/w3svc/AppPools", IID_IADsContainer, (void**)&spADsContainer);
	if(FAILED(hr))
	{
		oLogStream << _T("ADsGetObject '") << L"IIS://localhost/w3svc/AppPools" << _T("' failed; result is: ") << hr << _T(".") << std::endl;
		return;
	}

	hr = spADsContainer->Delete(CComBSTR(L"IIsApplicationPool"), CComBSTR(lpcwstrAppPool));
	if(FAILED(hr))
	{
		oLogStream << _T("Delete IIsApplicationPool'") << lpcwstrAppPool << _T("' failed; result is: ") << hr << _T(".") << std::endl;
		return;
	}

	if(SUCCEEDED(hr))
	{
		oLogStream << _T("DeleteAppPool '") << lpcwstrAppPool << _T("' succeeded.") << std::endl;
	}
}

void StopAppPool(std::ofstream_t& oLogStream, LPCWSTR lpcwstrAppPool)
{
	HRESULT hr;
	CComPtr<IISApplicationPool> spAppPool = NULL;

	std::wstring wsPath = L"IIS://LocalHost/W3SVC/AppPools/";
	wsPath += lpcwstrAppPool;
	hr = ADsGetObject(wsPath.c_str(), __uuidof(IISApplicationPool), (void**)&spAppPool);
	oLogStream << _T("ADsGetObject('") <<  wsPath << _T("') returned ") << hr << _T(".") << std::endl;
	if (SUCCEEDED(hr)) {
		hr = spAppPool->Stop();
		oLogStream << _T("spAppPool->Stop() '") <<  wsPath.c_str() << _T("' returned ") << hr << _T(".") << std::endl;
	}
}

void StartAppPool(std::ofstream_t& oLogStream, LPCWSTR lpcwstrAppPool)
{
	HRESULT hr;
	CComPtr<IISApplicationPool> spAppPool = NULL;

	std::wstring wsPath = L"IIS://LocalHost/W3SVC/AppPools/";
	wsPath+=lpcwstrAppPool;
	hr = ADsGetObject(wsPath.c_str(), __uuidof(IISApplicationPool), (void**)&spAppPool);
	oLogStream << _T("ADsGetObject('") <<  wsPath.c_str() << _T("') returned ") << hr << _T(".") << std::endl;
	if (SUCCEEDED(hr)) {
		hr = spAppPool->Start();
		oLogStream << _T("spAppPool->Start() '") <<  wsPath.c_str() << _T("' returned ") << hr << _T(".") << std::endl;
	}
}