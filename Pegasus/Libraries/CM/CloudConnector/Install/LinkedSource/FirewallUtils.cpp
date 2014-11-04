#include "StdAfx.h"
#include "FirewallUtils.h"

#include <netfw.h>
#include <atlbase.h>


#pragma comment( lib, "ole32.lib" )
#pragma comment( lib, "oleaut32.lib" )


int CleanupFirewallApplicationException(std::ofstream_t& oLogStream, const std::string_t& sImagePath)
{
	HRESULT hr = S_OK;

	CComPtr<INetFwMgr> spNetFwMgr = NULL;
	CComPtr<INetFwPolicy> spLocalPolicy = NULL;
	CComPtr<INetFwProfile> spProfile = NULL;
	CComPtr<INetFwAuthorizedApplications> spAuthorizedApps = NULL;

	// Retrieve INetFwPolicy2
	if(SUCCEEDED(hr))
	{
		hr = spNetFwMgr.CoCreateInstance(__uuidof(NetFwMgr), NULL, CLSCTX_INPROC_SERVER);
		if (FAILED(hr))
		{
			oLogStream << _T("CoCreateInstance(NetFwMgr) failed; result is: ") << hr << _T(".") << std::endl;
		}
	}

	// Retrieve INetFwRules
	if(SUCCEEDED(hr))
	{
		hr = spNetFwMgr->get_LocalPolicy(&spLocalPolicy);
		if (FAILED(hr))
		{
			oLogStream << _T("get_LocalPolicy failed; result is: ") << hr << _T(".") << std::endl;
		}
	}

	if(SUCCEEDED(hr))
	{
		hr = spLocalPolicy->get_CurrentProfile(&spProfile);
		if (FAILED(hr))
		{
			oLogStream << _T("get_CurrentProfile failed; result is: ") << hr << _T(".") << std::endl;
		}
	}

	if(SUCCEEDED(hr))
	{
		hr = spProfile->get_AuthorizedApplications(&spAuthorizedApps);
		if (FAILED(hr))
		{
			oLogStream << _T("get_AuthorizedApplications failed; result is: ") << hr << _T(".") << std::endl;
		}
	}

	// Remove the Firewall Rule
	if(SUCCEEDED(hr))
	{
		hr = spAuthorizedApps->Remove(CComBSTR(sImagePath.c_str()));
		if (FAILED(hr))
		{
			oLogStream << _T("Firewall authorized app remove failed; result is: ") << hr << _T(".") << std::endl;
		}
		else
		{
			oLogStream << _T("Firewall authorized app remove succeeded.") << std::endl;
		}
	}

	return 0;
}

int CleanupAdvancedFirewallRule(std::ofstream_t& oLogStream, const std::string_t& sRuleName)
{
	HRESULT hr = S_OK;

	CComPtr<INetFwPolicy2> spNetFwPolicy2 = NULL;
	CComPtr<INetFwRules> spFwRules = NULL;

	// Retrieve INetFwPolicy2
	if(SUCCEEDED(hr))
	{
		hr = spNetFwPolicy2.CoCreateInstance(__uuidof(NetFwPolicy2), NULL, CLSCTX_INPROC_SERVER);
		if (FAILED(hr))
		{
			oLogStream << _T("CoCreateInstance(NetFwPolicy2) failed; result is: ") << hr << _T(".") << std::endl;
		}
	}

	// Retrieve INetFwRules
	if(SUCCEEDED(hr))
	{
		hr = spNetFwPolicy2->get_Rules(&spFwRules);
		if (FAILED(hr))
		{
			oLogStream << _T("get_Rules failed; result is: ") << hr << _T(".") << std::endl;
		}
	}

	// Remove the Firewall Rule
	if(SUCCEEDED(hr))
	{
		hr = spFwRules->Remove(CComBSTR(sRuleName.c_str()));
		if (FAILED(hr))
		{
			oLogStream << _T("Firewall rule remove failed; result is: ") << hr << _T(".") << std::endl;
		}
		else
		{
			oLogStream << _T("Firewall rule remove succeeded.") << std::endl;
		}
	}

	return 0;
}