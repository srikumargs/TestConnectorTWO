#include "StdAfx.h"
#include <shlobj.h>
#include <Shlwapi.h>
#include "Configuration.h"
#include "IniFile.h"
#include "..\LinkedSource\Exceptions.h"

CConfiguration::CConfiguration(LPTSTR lpCmdLine, std::ofstream_t& oLogStream) 
: m_oLogStream(oLogStream),
m_dwExitFadeMS(0),
m_bShowWindowCaption(false)
{
	const DWORD PATH_SIZE = 4096;
	TCHAR tszTempName[MAX_PATH];
	TCHAR tszPath[PATH_SIZE];

	// get the temp file name for the diagnostic log
	GetTempPath(PATH_SIZE, tszPath);
	GetTempFileName(tszPath, _T("SB-"), 0, tszTempName);

	// create the log
	m_oLogStream.open(tszTempName);
	m_oLogStream << _T("Command line: ") << lpCmdLine << std::endl;
	::GetCurrentDirectory(PATH_SIZE, tszPath);
	m_oLogStream << _T("Working directory: ") << tszPath << std::endl;

	TCHAR tszExePath[MAX_PATH] = { 0 };
	GetModuleFileName(NULL, tszExePath, MAX_PATH);
	m_oLogStream << _T("szExePath: ") << tszExePath << std::endl;
	PathRemoveFileSpec(tszExePath);
	m_sExePath = tszExePath;

	// set default .ini file path
	::PathCombine(tszPath, m_sExePath.c_str(), _T("SageBrowser.ini"));
	m_sIniFilePath = tszPath;

	// add mappings for all options
	m_optionHandlers.insert(std::make_pair(std::string_t(_T("config")), &CConfiguration::ConfigurationOptionHandler));

	// process all command-line options
	StringVector sCommandLine = Tokenize(std::string_t(lpCmdLine), _T("/"));
	for(StringVector::size_type i = 0;i < sCommandLine.size(); i++)
	{
		std::string_t sOption = TrimSpaces(sCommandLine[i]);
		std::string_t sParams;
		std::string_t::size_type nPos = sOption.find_first_of(_T(":"));
		if(nPos != std::string_t::npos)
		{
			sParams = TrimSpaces(sOption.substr(nPos + 1));
			sOption = sOption.substr(0, nPos);
		}

		// invoke the handler for this option
		m_oLogStream << "Processing option '" << sOption << "'" << std::endl;
		OptionHandlerMap::iterator iterFind = m_optionHandlers.find(sOption);
		if(iterFind != m_optionHandlers.end())
		{
			((*this).*(iterFind->second))(sParams);
		}
	}

	m_oLogStream << "Loading configuration '" << m_sIniFilePath << "'" << std::endl;
	CIniFile oIniFile(m_sIniFilePath.c_str());
	oIniFile.Load();

	m_oLogStream << "Reading General section from configuration" << std::endl;
	m_sWindowCaption = oIniFile.ReadString(_T("General"), _T("WindowCaption"), _T("Sage Browser"));
	m_sImageFilePath = ExpandToFullPath(oIniFile.ReadString(_T("General"), _T("ImageFilePath"), _T("SageBrowser.bmp")));
	m_dwExitFadeMS = oIniFile.ReadUint(_T("General"), _T("ExitFadeMS"), 1000);
	m_bShowWindowCaption = (oIniFile.ReadUint(_T("General"), _T("ShowWindowCaption"), 0) != 0);

	m_oLogStream << "Reading TextAttributes sections from configuration" << std::endl;
	typedef std::map<std::string_t, CTextAttributes> TextAttributesContainer;
	TextAttributesContainer textAttributes;
	StringVector oKeys = oIniFile.GetKeysForSection(_T("TextAttributes"));
	for(StringVector::size_type i = 0 ; i < oKeys.size() ; i++)
	{
		const std::string_t& sSection = oIniFile.ReadString(_T("TextAttributes"), oKeys[i], _T(""));
		m_oLogStream << "Reading '" << sSection << "' TextAttribute section from configuration" << std::endl;
		int nHeight = oIniFile.ReadUint(sSection, _T("Height"), 0);
		int nWidth = oIniFile.ReadUint(sSection, _T("Width"), 0);
		int nWeight = oIniFile.ReadUint(sSection, _T("Weight"), 0);
		bool bItalic = (oIniFile.ReadUint(sSection, _T("Italic"), 0) != 0);
		bool bUnderline = (oIniFile.ReadUint(sSection, _T("bUnderline"), 0) != 0);
		std::string_t sFace = oIniFile.ReadString(sSection, _T("Face"), _T("Arial"));

		DWORD dwTextColor = StringToDWORD(oIniFile.ReadString(sSection, _T("TextColor"), _T("")), 16);
		DWORD dwBackgroundColor = StringToDWORD(oIniFile.ReadString(sSection, _T("BackgroundColor"), _T("")), 16);
		DWORD dwHoverBackgroundColor = StringToDWORD(oIniFile.ReadString(sSection, _T("HoverBackgroundColor"), _T("")), 16);

		textAttributes.insert(std::make_pair<std::string_t, CTextAttributes>(sSection, CTextAttributes(nHeight, nWidth, nWeight, bItalic, bUnderline, sFace, dwTextColor, dwBackgroundColor, dwHoverBackgroundColor)));
	}

	m_oLogStream << "Reading DynamicText sections from configuration" << std::endl;
	oKeys.clear();
	oKeys = oIniFile.GetKeysForSection(_T("DynamicText"));
	for(StringVector::size_type i = 0 ; i < oKeys.size() ; i++)
	{
		const std::string_t& sSection = oIniFile.ReadString(_T("DynamicText"), oKeys[i], _T(""));
		m_oLogStream << "Reading '" << sSection << "' DynamicText section from configuration" << std::endl;
		DWORD dwX = oIniFile.ReadUint(sSection, _T("X"), 0);
		DWORD dwY = oIniFile.ReadUint(sSection, _T("Y"), 0);
		std::string_t sText = oIniFile.ReadString(sSection, _T("Text"), _T(""));
		CommandType eCommandType = (CommandType) oIniFile.ReadUint(sSection, _T("CommandType"), ctNone);
		std::string_t sCommand = oIniFile.ReadString(sSection, _T("Command"), _T(""));
		if(eCommandType == ctShellExecOpen && 
			sCommand.find(_T("http://")) == std::string_t::npos && 
			sCommand.find(_T("https://")) == std::string_t::npos && 
			::PathIsRelative(sCommand.c_str()))
		{
			sCommand = ::PathCombine(tszPath, m_sExePath.c_str(), sCommand.c_str());
		}
		std::string_t sParameters = oIniFile.ReadString(sSection, _T("Parameters"), _T(""));
		std::string_t sTextAttributes = oIniFile.ReadString(sSection, _T("TextAttributes"), _T(""));

		m_dynamicTextInfo.push_back(CDynamicTextInfo(dwX, dwY, sText, eCommandType, sCommand, sParameters, textAttributes[sTextAttributes]));
	}
}

void CConfiguration::ConfigurationOptionHandler(const std::string_t& sParams)
{
	if(sParams.empty())
	{
		throw CInvalidCommandLineParameterException(_T("CConfiguration::ConfigurationOptionHandler"), _T("config"), sParams);
	}

	m_sIniFilePath = ExpandToFullPath(sParams);
}

std::string_t CConfiguration::ExpandToFullPath(const std::string_t& sPath)
{
	if(sPath.empty())
	{
		throw CStringArgumentNullOrEmptyException(_T("CConfiguration::ExpandToFullPath"), _T("sPath"));
	}

	std::string_t sResult = sPath;

	if(::PathIsRelative(sResult.c_str()))
	{
		sResult = m_sExePath + _T("\\") + sPath;
	}

	TCHAR tszCanonicalPath[MAX_PATH];
	::PathCanonicalize(tszCanonicalPath, sResult.c_str());
	sResult = tszCanonicalPath;

	return sResult;
}