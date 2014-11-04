#pragma once

#include "..\LinkedSource\StringUtils.h"
#include <map>

class CVariablesHandler
{
public:
	CVariablesHandler();

	std::string_t ExpandAllVariables(const std::string_t& sExpression);

private:
	std::string_t ReplaceRegistryVariable(const std::string_t& sExpression);
	std::string_t GetRegistryValue(const std::string_t& sKeyPath, const std::string_t& sValueName);
	std::string_t ReplaceFolderPathVariable(const std::string_t& sExpression);
	std::string_t GetFolderPath(const std::string_t& sFolderId);
	std::string_t ReplaceEnvironmentVariable(const std::string_t& sExpression);
	std::string_t ReplaceUserEnvironmentVariable(const std::string_t& sExpression);
	std::string_t ReplaceSystemEnvironmentVariable(const std::string_t& sExpression);
	std::string_t GetSystemEnvironmentVariable(const std::string_t& variableName);
	std::string_t GetUserEnvironmentVariable(const std::string_t& variableName);

	void VerifyAllVariablesReplaced(const std::string_t& sExpression);

	typedef std::map<std::string_t, HKEY> HkeyMap;
	HkeyMap m_hkeyRoots;

	typedef std::map<std::string_t, int> FolderMap;
	FolderMap m_folders;

	// disallow assignment
	CVariablesHandler& operator=(const CVariablesHandler&);
};
