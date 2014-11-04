#pragma once
#include "..\LinkedSource\StringUtils.h"
#include <list>
#include <map>
#include <fstream>

class CRunningProcess
{
public:
	CRunningProcess(DWORD dwProcessID, const std::string_t& strProcessName, const std::string_t& strProcessFileName)
		:m_dwProcessID(dwProcessID),
		m_strProcessName(strProcessName),
		m_strProcessFileName(strProcessFileName)
	{}

	const std::string_t& Name() const
	{return m_strProcessName;}
	const std::string_t& FileName() const
	{return m_strProcessFileName;}
	DWORD ID() const
	{return m_dwProcessID;}

private:
	DWORD  m_dwProcessID;
	std::string_t m_strProcessName;
	std::string_t m_strProcessFileName;
};

class CRunningProcessManager
{
public:
	CRunningProcessManager()
	{}

	void StopRunningProcesses(std::ofstream_t& oLogStream, const std::string_t& sDir);

private:
	void GetProcessInfo(DWORD dwProcessID);
	void Lowercase(std::string_t& str);

	typedef std::list<CRunningProcess> 									RunningProcessesContainer;
	typedef std::map<std::string_t /*module name*/, RunningProcessesContainer>	ModulesInUseContainer;

	ModulesInUseContainer	m_ModulesInUse;
};
