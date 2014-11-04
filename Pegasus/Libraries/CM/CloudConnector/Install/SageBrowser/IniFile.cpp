#include "StdAfx.h"
#include "IniFile.h"

CIniFile::CIniFile(const TCHAR* szFileName) : m_sFileName(szFileName)
{}

bool CIniFile::Load()
{
	try
	{
		std::ifstream_t ifs(m_sFileName.c_str());

		std::string_t sLine;
		while(getline(ifs, sLine))
		{
			m_lines.push_back(sLine);
		}
	}
	catch(...)
	{
		OutputDebugString(_TEXT( "ERROR:  Unhandled exception in CIniFile::Load.\n" ));
		return false;
	}

	return true;
}

bool CIniFile::Save()
{
	try
	{
		std::ofstream_t ofs(m_sFileName.c_str());

		LineContainer::iterator iter = m_lines.begin();
		while(iter != m_lines.end())
		{
			ofs << (*iter) << _T("\n");
			iter++;
		}
	}
	catch(...)
	{
		OutputDebugString(_TEXT( "ERROR:  Unhandled exception in CIniFile::Save.\n" ));
		return false;
	}

	return true;
}

DWORD CIniFile::ReadUint(const std::string_t& sSection, const std::string_t& sKey, DWORD nDefault)
{
	DWORD nResult = nDefault;
	try
	{
		LineContainer::iterator iter = FindLine(sSection, sKey);
		if(m_lines.end() != iter)
		{
			std::string_t::size_type indexValue = (*iter).find_first_not_of(_T(" \t"), (*iter).find_first_of(_T("="), sKey.length()));
			if(std::string::npos != indexValue)
			{
				nResult = _tstoi((*iter).substr(indexValue + 1, (*iter).find_last_not_of(_T(" \t;")) - indexValue).c_str());
			}
		}
	}
	catch(...)
	{
		OutputDebugString(_TEXT( "ERROR:  Unhandled exception in CIniFile::ReadUInt.\n" ));
	}

	return nResult;
}

std::string_t CIniFile::ReadString(const std::string_t& sSection, const std::string_t& sKey, const std::string_t& sDefault)
{
	std::string_t sResult = sDefault;
	try
	{
		if(sSection.empty() || sKey.empty())
		{
			// TOOD: implement NULL section and/or NULL key paths
			OutputDebugString(_TEXT("ERROR:  null params not implemented in CIniFile::ReadString()\n"));
			return sResult;
		}
		LineContainer::iterator iter = FindLine(sSection, sKey);
		if(m_lines.end() != iter)
		{
			std::string_t::size_type indexValue = (*iter).find_first_not_of(_T(" \t"), (*iter).find_first_of(_T("="), sKey.length()));
			if(std::string::npos != indexValue)
			{
				sResult = std::string_t((*iter).substr(indexValue + 1, (*iter).find_last_not_of(_T(" \t;")) - indexValue));
			}
		}
	}
	catch(...)
	{
		OutputDebugString(_TEXT( "ERROR:  Unhandled exception in CIniFile::ReadString.\n" ));
	}

	return sResult;
}   

bool CIniFile::WriteString(const TCHAR* szSection, const TCHAR* szKey, const TCHAR* szString)
{
	bool bResult = false;
	try
	{
		if(NULL == szKey || NULL == szString)
		{
			OutputDebugString(_TEXT("ERROR:  null paraams not implemented in CIniFile::WriteString()\n"));
			return false;
		}

		LineContainer::iterator iter = FindLine(szSection, szKey); 
		std::string_t sNewLine = std::string_t(szKey) + _T("=") + szString;
		if(m_lines.end() != iter)
		{
			(*iter) = sNewLine;
		}
		else
		{
			LineContainer::iterator iterSection = FindSection(szSection);
			if(m_lines.end() != iterSection)
			{
				// section exists, but key doesn't
				m_lines.insert(iterSection + 1, sNewLine);
			}
			else
			{
				// neither section nor line exist
				m_lines.push_back(std::string_t(_T("[")) + szSection + _T("]"));
				m_lines.push_back(sNewLine);
			}
		}
		bResult = true;

	}
	catch(...)
	{
		OutputDebugString(_TEXT( "ERROR:  Unhandled exception in CIniFile::WriteString.\n" ));
	}
	return bResult;
}

StringVector CIniFile::GetKeysForSection(const TCHAR* tszSection)
{
	StringVector oResult;

	LineContainer::iterator iter = FindSection(tszSection);
	if(iter != m_lines.end())
	{
		iter++;
		while(iter != m_lines.end() && !StartsWith((*iter), _T("[")))
		{
			if(!(*iter).empty())
			{
				oResult.push_back((*iter).substr(0, (*iter).find_first_of(_T(" \t="))));
			}
			iter++;
		}
	}

	return oResult;
}

CIniFile::LineContainer::iterator CIniFile::FindSection(const std::string_t& sSectionId)
{
	LineContainer::iterator iterResult = m_lines.end();
	try
	{
		LineContainer::iterator iter = m_lines.begin();
		const std::string_t sSection = std::string_t(_T("[")) + std::string_t(sSectionId) + std::string_t(_T("]"));
		while(m_lines.end() != iter)
		{
			if(0 == (*iter).find(sSection))
			{
				iterResult = iter;
				break;
			}
			iter++;
		}        
	}
	catch(...)
	{
		OutputDebugString(_TEXT( "ERROR:  Unhandled exception in CIniFile::FindSection.\n" ));
	}

	return iterResult;
}

CIniFile::LineContainer::iterator CIniFile::FindLine(const std::string_t& sSection, const std::string_t& sKey)
{
	LineContainer::iterator iterResult = m_lines.end();
	try
	{
		LineContainer::iterator iter = FindSection(sSection);
		if(m_lines.end() != iter)
		{
			iter++;
			while(m_lines.end() != iter)
			{
				if(LineMatchesKey((*iter), sKey))
				{
					// found the key
					iterResult = iter;
					break;
				}
				iter++;
			}
		}
	}
	catch(...)
	{
		OutputDebugString(_TEXT( "ERROR:  Unhandled exception in CIniFile::FindLine.\n" ));
	}

	return iterResult;
}

bool CIniFile::LineMatchesKey(const std::string_t& sLine, const std::string_t& sKey)
{
	bool bResult = false;

	if(0 == sLine.find(sKey))
	{
		// in the right section, and starts with szKey ... make sure it is indeed the right one
		std::string_t::size_type indexNonWhite = sLine.find_first_not_of(_T(" \t"), sKey.length());
		if(std::string_t::npos != indexNonWhite && sLine[indexNonWhite] == _T('='))
		{
			bResult = true;
		}                
	}

	return bResult;
}
