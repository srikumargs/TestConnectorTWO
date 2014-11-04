#pragma once
#include <vector>
#include "..\LinkedSource\StringUtils.h"

class CIniFile
{
public:
	CIniFile(const TCHAR* tszFileName);

	bool Load();
	bool Save();

	DWORD ReadUint(const std::string_t& sSection, const std::string_t& sKey, DWORD dwDefault);
	std::string_t ReadString(const std::string_t& sSection, const std::string_t& sKey, const std::string_t& sDefault);
	bool WriteString(const TCHAR* tszSection, const TCHAR* tszKey, const TCHAR* tszString);
	StringVector GetKeysForSection(const TCHAR* tszSection);

private:
	typedef std::vector<std::string_t> LineContainer;

	LineContainer::iterator FindSection(const std::string_t& sSectionId);
	LineContainer::iterator FindLine(const std::string_t& sSection, const std::string_t& sKey);
	bool LineMatchesKey(const std::string_t& sLine, const std::string_t& sKey);

	std::string_t m_sFileName;
	LineContainer m_lines; 

	// Disallow copying and assignment
	CIniFile(const CIniFile&);
	const CIniFile& operator=(const CIniFile&);
};
