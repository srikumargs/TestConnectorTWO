#include "StdAfx.h"
#include "VariablesHandler.h"
#include <functional>
#include <algorithm>
#include <vector>
#include <iterator>
#include <regex>
#include <shlobj.h>
#include <Shlwapi.h>
#include "..\LinkedSource\Exceptions.h"

CVariablesHandler::CVariablesHandler()
{}

std::string_t CVariablesHandler::ExpandAllVariables(const std::string_t& sExpression)
{
	std::string_t sResult = sExpression;

	bool bSomethingReplaced = false;
	do
	{
		bSomethingReplaced = false;

		// Performance optimization:  rather than reading the entire registry up front and storing
		// all values as LibraryConfigTool variables, we read/replace registry values just-in-time.
		// Supported syntax for registry variables is:
		//
		//   $(Environment::Registry::<KeyName>\<ValueName>)
		//
		// E.g.,  $(Environment::Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Microsoft SDKs\Windows\CurrentInstallFolder)
		// evaluates to the value of CurrentInstallFolder in the "SOFTWARE\Microsoft\Microsoft SDKs\Windows" key
		// under the HKLM hive.
		std::string_t sNewResult = ReplaceRegistryVariable(sResult);
		if (sNewResult != sResult)
		{
			sResult = sNewResult;
			bSomethingReplaced = true;
		}

		// Supported syntax for special folder path variables is:
		//
		//   $(Environment::Folder::<Special Folder Path ID>)
		//
		// E.g., $(Environment::Folder::MYDOCUMENTS) evaluates to the value of the current user's My Documents location.
		if (!bSomethingReplaced)
		{
			sNewResult = ReplaceFolderPathVariable(sResult);
			if (sNewResult != sResult)
			{
				sResult = sNewResult;
				bSomethingReplaced = true;
			}
		}

		// Supported syntax for environment variables is:
		//
		//   $(Environment::[User|System]::<Variable Name>)
		//
		// E.g., $(Environment::System::WINDR) evaluates to the value of the %WINDIR% system environmment variable.
		if (!bSomethingReplaced)
		{
			sNewResult = ReplaceEnvironmentVariable(sResult);
			if (sNewResult != sResult)
			{
				sResult = sNewResult;
				bSomethingReplaced = true;
			}
		}
	}
	while(bSomethingReplaced);

	VerifyAllVariablesReplaced(sResult);

	return sResult;
}

std::string_t CVariablesHandler::ReplaceRegistryVariable(const std::string_t& sExpression)
{
	std::string_t sResult = sExpression;

	std::tr1::cmatch_t match;
	std::tr1::regex_t regex(_T("(.*)(\\$\\(Environment::Registry::[^\\$\\(\\):]+\\))(.*)"));
	std::tr1::regex_search(sExpression.c_str(), match, regex);
	if(match.size() > 0)
	{
		std::string_t prefix = _T("$(Environment::Registry::");
		std::string_t valuePath = match[2];
		valuePath = TrimEnd(valuePath.substr(prefix.size()), _T(")"));
		std::string_t keyName = valuePath.substr(0, valuePath.find_last_of(_T("\\")));
		std::string_t valueName = valuePath.substr(keyName.size() + 1);
		std::string_t value = GetRegistryValue(keyName, valueName);
		sResult = Replace(sResult, match[2], value);
	}

	return sResult;
}

std::string_t CVariablesHandler::GetRegistryValue(const std::string_t& sKeyPath, const std::string_t& sValueName)
{
	std::string_t sResult;

	if(m_hkeyRoots.size() == 0)
	{
		m_hkeyRoots[std::string_t(_T("HKCR"))] = HKEY_CLASSES_ROOT;
		m_hkeyRoots[std::string_t(_T("HKEY_CLASSES_ROOT"))] = HKEY_CLASSES_ROOT;
		m_hkeyRoots[std::string_t(_T("HKCU"))] = HKEY_CURRENT_USER;
		m_hkeyRoots[std::string_t(_T("HKEY_CURRENT_USER"))] = HKEY_CURRENT_USER;
		m_hkeyRoots[std::string_t(_T("HKLM"))] = HKEY_LOCAL_MACHINE;
		m_hkeyRoots[std::string_t(_T("HKEY_LOCAL_MACHINE"))] = HKEY_LOCAL_MACHINE;
	}

	std::string_t rootKeyString = sKeyPath.substr(0, sKeyPath.find_first_of(_T("\\")));
	std::string_t subKeyString = sKeyPath.substr(sKeyPath.find_first_of(_T("\\")) + 1);

	HkeyMap::iterator iterFind = m_hkeyRoots.find(rootKeyString);
	if(iterFind != m_hkeyRoots.end())
	{
		HKEY key;
		DWORD dwBufferSize = 8192;
		DWORD cbData;
		DWORD dwRet = RegOpenKeyEx((*iterFind).second, subKeyString.c_str(), 0, KEY_READ, &key);
		if(dwRet == ERROR_SUCCESS)
		{
			LPBYTE byData = (LPBYTE) malloc( dwBufferSize );
			cbData = dwBufferSize;

			dwRet = RegQueryValueEx( key,
				sValueName.c_str(),
				NULL,
				NULL,
				(LPBYTE) byData,
				&cbData );
			while( dwRet == ERROR_MORE_DATA )
			{
				// Get a buffer that is big enough.
				dwBufferSize += 4096;
				byData = (LPBYTE) realloc( byData, dwBufferSize );
				cbData = dwBufferSize;

				dwRet = RegQueryValueEx( key,
					sValueName.c_str(),
					NULL,
					NULL,
					(LPBYTE) byData,
					&cbData );
			}

			if( dwRet == ERROR_SUCCESS )
			{
				sResult = (LPCTSTR) byData;
			}
			RegCloseKey(key);
		}
	}

	return sResult;
}

std::string_t CVariablesHandler::ReplaceFolderPathVariable(const std::string_t& sExpression)
{
	std::string_t sResult = sExpression;

	std::tr1::cmatch_t match;
	std::tr1::regex_t regex(_T("(.*)(\\$\\(Environment::Folder::[^\\$\\(\\):]+\\))(.*)"));
	std::tr1::regex_search(sExpression.c_str(), match, regex);
	if(match.size() > 0)
	{
		std::string_t prefix = _T("$(Environment::Folder::");
		std::string_t valuePath = match[2];
		std::string_t folderId = TrimEnd(valuePath.substr(prefix.size()), _T(")"));
		std::string_t value = GetFolderPath(folderId);
		sResult = Replace(sResult, match[2], value);
	}

	return sResult;
}

std::string_t CVariablesHandler::GetFolderPath(const std::string_t& sFolderId)
{
	std::string_t sResult;

	if(m_folders.size() == 0)
	{
		m_folders[std::string_t(_T("ADMINTOOLS"))] = CSIDL_ADMINTOOLS;
		m_folders[std::string_t(_T("ALTSTARTUP"))] = CSIDL_ALTSTARTUP;
		m_folders[std::string_t(_T("APPDATA"))] = CSIDL_APPDATA;
		m_folders[std::string_t(_T("BITBUCKET"))] = CSIDL_BITBUCKET;
		m_folders[std::string_t(_T("CDBURN_AREA"))] = CSIDL_CDBURN_AREA;
		m_folders[std::string_t(_T("COMMON_ADMINTOOLS"))] = CSIDL_COMMON_ADMINTOOLS;
		m_folders[std::string_t(_T("COMMON_ALTSTARTUP"))] = CSIDL_COMMON_ALTSTARTUP;
		m_folders[std::string_t(_T("COMMON_APPDATA"))] = CSIDL_COMMON_APPDATA;
		m_folders[std::string_t(_T("COMMON_DESKTOPDIRECTORY"))] = CSIDL_COMMON_DESKTOPDIRECTORY;
		m_folders[std::string_t(_T("COMMON_DOCUMENTS"))] = CSIDL_COMMON_DOCUMENTS;
		m_folders[std::string_t(_T("COMMON_FAVORITES"))] = CSIDL_COMMON_FAVORITES;
		m_folders[std::string_t(_T("COMMON_MUSIC"))] = CSIDL_COMMON_MUSIC;
		m_folders[std::string_t(_T("COMMON_OEM_LINKS"))] = CSIDL_COMMON_OEM_LINKS;
		m_folders[std::string_t(_T("COMMON_PICTURES"))] = CSIDL_COMMON_PICTURES;
		m_folders[std::string_t(_T("COMMON_PROGRAMS"))] = CSIDL_COMMON_PROGRAMS;
		m_folders[std::string_t(_T("COMMON_STARTMENU"))] = CSIDL_COMMON_STARTMENU;
		m_folders[std::string_t(_T("COMMON_STARTUP"))] = CSIDL_COMMON_STARTUP;
		m_folders[std::string_t(_T("COMMON_TEMPLATES"))] = CSIDL_COMMON_TEMPLATES;
		m_folders[std::string_t(_T("COMMON_VIDEO"))] = CSIDL_COMMON_VIDEO;
		m_folders[std::string_t(_T("COMPUTERSNEARME"))] = CSIDL_COMPUTERSNEARME;
		m_folders[std::string_t(_T("CONNECTIONS"))] = CSIDL_CONNECTIONS;
		m_folders[std::string_t(_T("CONTROLS"))] = CSIDL_CONTROLS;
		m_folders[std::string_t(_T("COOKIES"))] = CSIDL_COOKIES;
		m_folders[std::string_t(_T("DESKTOP"))] = CSIDL_DESKTOP;
		m_folders[std::string_t(_T("DESKTOPDIRECTORY"))] = CSIDL_DESKTOPDIRECTORY;
		m_folders[std::string_t(_T("DRIVES"))] = CSIDL_DRIVES;
		m_folders[std::string_t(_T("FAVORITES"))] = CSIDL_FAVORITES;
		m_folders[std::string_t(_T("FONTS"))] = CSIDL_FONTS;
		m_folders[std::string_t(_T("HISTORY"))] = CSIDL_HISTORY;
		m_folders[std::string_t(_T("INTERNET"))] = CSIDL_INTERNET;
		m_folders[std::string_t(_T("INTERNET_CACHE"))] = CSIDL_INTERNET_CACHE;
		m_folders[std::string_t(_T("LOCAL_APPDATA"))] = CSIDL_LOCAL_APPDATA;
		m_folders[std::string_t(_T("MYDOCUMENTS"))] = CSIDL_MYDOCUMENTS;
		m_folders[std::string_t(_T("MYMUSIC"))] = CSIDL_MYMUSIC;
		m_folders[std::string_t(_T("MYPICTURES"))] = CSIDL_MYPICTURES;
		m_folders[std::string_t(_T("MYVIDEO"))] = CSIDL_MYVIDEO;
		m_folders[std::string_t(_T("NETHOOD"))] = CSIDL_NETHOOD;
		m_folders[std::string_t(_T("NETWORK"))] = CSIDL_NETWORK;
		m_folders[std::string_t(_T("PERSONAL"))] = CSIDL_PERSONAL;
		m_folders[std::string_t(_T("PRINTERS"))] = CSIDL_PRINTERS;
		m_folders[std::string_t(_T("PRINTHOOD"))] = CSIDL_PRINTHOOD;
		m_folders[std::string_t(_T("PROFILE"))] = CSIDL_PROFILE;
		m_folders[std::string_t(_T("PROGRAM_FILES"))] = CSIDL_PROGRAM_FILES;
		m_folders[std::string_t(_T("PROGRAM_FILESX86"))] = CSIDL_PROGRAM_FILESX86;
		m_folders[std::string_t(_T("PROGRAM_FILES_COMMON"))] = CSIDL_PROGRAM_FILES_COMMON;
		m_folders[std::string_t(_T("PROGRAM_FILES_COMMONX86"))] = CSIDL_PROGRAM_FILES_COMMONX86;
		m_folders[std::string_t(_T("PROGRAMS"))] = CSIDL_PROGRAMS;
		m_folders[std::string_t(_T("RECENT"))] = CSIDL_RECENT;
		m_folders[std::string_t(_T("RESOURCES"))] = CSIDL_RESOURCES;
		m_folders[std::string_t(_T("RESOURCES_LOCALIZED"))] = CSIDL_RESOURCES_LOCALIZED;
		m_folders[std::string_t(_T("SENDTO"))] = CSIDL_SENDTO;
		m_folders[std::string_t(_T("STARTMENU"))] = CSIDL_STARTMENU;
		m_folders[std::string_t(_T("STARTUP"))] = CSIDL_STARTUP;
		m_folders[std::string_t(_T("SYSTEM"))] = CSIDL_SYSTEM;
		m_folders[std::string_t(_T("SYSTEMX86"))] = CSIDL_SYSTEMX86;
		m_folders[std::string_t(_T("TEMPLATES"))] = CSIDL_TEMPLATES;
		m_folders[std::string_t(_T("WINDOWS"))] = CSIDL_WINDOWS;
	}

	FolderMap::iterator iterFind = m_folders.find(sFolderId);
	if(iterFind != m_folders.end())
	{
		TCHAR pszPath[MAX_PATH + 1];

		if(S_OK == SHGetFolderPath(NULL, (*iterFind).second, NULL, SHGFP_TYPE_CURRENT, pszPath))
		{
			sResult = pszPath;
		}
	}

	return sResult;
}

std::string_t CVariablesHandler::ReplaceEnvironmentVariable(const std::string_t& sExpression)
{
	std::string_t sResult = sExpression;
	sResult = ReplaceSystemEnvironmentVariable(sResult);
	sResult = ReplaceUserEnvironmentVariable(sResult);
	return sResult;
}

std::string_t CVariablesHandler::ReplaceSystemEnvironmentVariable(const std::string_t& sExpression)
{
	std::string_t sResult = sExpression;

	std::tr1::cmatch_t match;
	std::tr1::regex_t regex(_T("(.*)(\\$\\(Environment::System::[^\\$\\(\\):]+\\))(.*)"));
	std::tr1::regex_search(sExpression.c_str(), match, regex);
	if(match.size() > 0)
	{
		std::string_t prefix = _T("$(Environment::System::");
		std::string_t valuePath = match[2];
		std::string_t variableName = TrimEnd(valuePath.substr(prefix.size()), _T(")"));
		std::string_t value = GetSystemEnvironmentVariable(variableName);
		sResult = Replace(sResult, match[2], value);
	}

	return sResult;
}

std::string_t CVariablesHandler::ReplaceUserEnvironmentVariable(const std::string_t& sExpression)
{
	std::string_t sResult = sExpression;

	std::tr1::cmatch_t match;
	std::tr1::regex_t regex(_T("(.*)(\\$\\(Environment::User::[^\\$\\(\\):]+\\))(.*)"));
	std::tr1::regex_search(sExpression.c_str(), match, regex);
	if(match.size() > 0)
	{
		std::string_t prefix = _T("$(Environment::User::");
		std::string_t valuePath = match[2];
		std::string_t variableName = TrimEnd(valuePath.substr(prefix.size()), _T(")"));
		std::string_t value = GetUserEnvironmentVariable(variableName);
		sResult = Replace(sResult, match[2], value);
	}

	return sResult;
}

std::string_t CVariablesHandler::GetSystemEnvironmentVariable(const std::string_t& variableName)
{
	return GetRegistryValue(_T("HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Control\\Session Manager\\Environment"), variableName);
}

std::string_t CVariablesHandler::GetUserEnvironmentVariable(const std::string_t& variableName)
{
	return GetRegistryValue(_T("HKEY_CURRENT_USER\\Environment"), variableName);
}

void CVariablesHandler::VerifyAllVariablesReplaced(const std::string_t& sExpression)
{
	std::tr1::cmatch_t match;
	std::tr1::regex_t regex(_T("\\$\\(.*\\)"));
	std::tr1::regex_search(sExpression.c_str(), match, regex);
	if(match.size() > 0)
	{
		throw CUnresolvedVariablesException(_T("CVariablesHandler::VerifyAllVariablesReplaced"), sExpression);
	}
}