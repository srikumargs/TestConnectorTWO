#pragma once
#include "..\LinkedSource\StringUtils.h"
#include <map>
#include <fstream>
#include "DynamicTextInfo.h"

class CConfiguration
{
public:
	typedef std::vector<CDynamicTextInfo> DynamicTextInfoContainer;

	CConfiguration(LPTSTR lpCmdLine, std::ofstream_t& oLogStream);

	DWORD GetExitFadeMS() const
	{ return m_dwExitFadeMS; }

	const DynamicTextInfoContainer& GetDynamicTextInfo() const
	{ return m_dynamicTextInfo; }

	const std::string_t GetWindowCaption() const
	{ return m_sWindowCaption; }

	bool GetShowWindowCaption() const
	{ return m_bShowWindowCaption; }

	std::ofstream_t& GetLogStream() const
	{ return m_oLogStream; }

	const std::string_t& GetImageFilePath() const
	{ return m_sImageFilePath; }

	bool GetAutoUpgrade() const
	{return m_bAutoUpgrade;}

	std::string_t ExpandToFullPath(const std::string_t& sPath) const;

private:
	void ConfigurationOptionHandler(const std::string_t& sParams);
	void AutoUpgradeOptionHandler(const std::string_t& sParams);

	typedef std::map<std::string_t, void(CConfiguration::*)(const std::string_t&), std::less<std::string_t> > OptionHandlerMap;
	OptionHandlerMap m_optionHandlers;

	std::ofstream_t& m_oLogStream;
	std::string_t m_sExePath;
	std::string_t m_sIniFilePath;
	std::string_t m_sImageFilePath;
	DWORD m_dwExitFadeMS;
	DynamicTextInfoContainer m_dynamicTextInfo;
	std::string_t m_sWindowCaption;
	bool m_bShowWindowCaption;
	bool m_bAutoUpgrade;

	// Disallow copying and assignment
	CConfiguration(const CConfiguration&);
	CConfiguration& operator=(const CConfiguration&);
};
