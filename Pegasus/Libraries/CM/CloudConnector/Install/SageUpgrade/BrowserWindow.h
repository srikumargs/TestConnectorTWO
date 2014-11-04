#pragma once
#include <iostream>
#include <string>
#include <list>
#include "..\LinkedSource\StringUtils.h"
#include "..\LinkedSource\SharedPtr.h"
#include "Configuration.h"
#include "Hyperlink.h"

const UINT  WM_UPGRADE_STARTING							= WM_USER + 1;
const UINT  WM_UPGRADE_STOP_MONITOR_SERVICE				= WM_USER + 2;
const UINT  WM_UPGRADE_WAIT_MONITOR_SERVICE_NOT_READY	= WM_USER + 3;
const UINT  WM_UPGRADE_STOP_CONNECTOR_SERVICE			= WM_USER + 4;
const UINT  WM_UPGRADE_WAIT_CONNECTOR_SERVICE_NOT_READY	= WM_USER + 5;
const UINT  WM_UPGRADE_PREPARE_PRODUCT					= WM_USER + 6;
const UINT  WM_UPGRADE_PREPARE_STOP_RUNNING_PROCS		= WM_USER + 7;
const UINT  WM_UPGRADE_PREPARE_SQL_CE_40_SP1			= WM_USER + 8;
const UINT  WM_UPGRADE_SQL_CE_40_SP1					= WM_USER + 9;
const UINT  WM_UPGRADE_BACKUP_DATABASE					= WM_USER + 10;
const UINT  WM_UPGRADE_PRODUCT							= WM_USER + 11;
const UINT  WM_UPGRADE_RECONFIGURE_CONNECTOR_SERVICE    = WM_USER + 12;
const UINT  WM_UPGRADE_RECONFIGURE_MONITOR_SERVICE      = WM_USER + 13;
const UINT  WM_UPGRADE_PRIOR_DATABASE					= WM_USER + 14;
const UINT  WM_UPGRADE_FINISHING						= WM_USER + 15;
const UINT  WM_UPGRADE_START_CONNECTOR_SERVICE			= WM_USER + 16;
const UINT  WM_UPGRADE_WAIT_CONNECTOR_SERVICE_READY		= WM_USER + 17;
const UINT  WM_UPGRADE_START_MONITOR_SERVICE			= WM_USER + 18;
const UINT  WM_UPGRADE_WAIT_MONITOR_SERVICE_READY		= WM_USER + 19;
const UINT  WM_UPGRADE_FINISHING_SUMMARY				= WM_USER + 20;

class CBrowserWindow
{
public:
	CBrowserWindow(HINSTANCE hInstance, const CConfiguration& oConfig);

	void Show();

	void ShowStatus(LPCTSTR tszStatus);
	void AdvanceUpgradeState(DWORD dwState, LPCTSTR tstrStatus);
	void DoUpgradeStateDelayed(UINT uElapsed);

	BOOL IsUpgradeInProgress() const {return m_bIsUpgradeInProgress;}
	HWND GetHWND() const {return m_hwnd;}

private:
	void RegisterWindowClass();
	void UnregisterWindowClass();

	HWND CreateBrowserWindow();
	void SetBackgroundImage(HWND hwnd, HBITMAP hbmp);
	void SetupLinkedText(HWND hwnd);
	void FadeWindowOut(HWND hwnd);
	DWORD PumpMsgWaitForMultipleObjects(HWND hwnd, DWORD dwCount, LPHANDLE pHandles, DWORD dwMilliseconds);
	static LRESULT CALLBACK CBrowserWindow::WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);

	std::string_t m_sBrowserWindowClass;
	HINSTANCE m_hInstance;
	DWORD m_dwFadeoutEnd;
	HANDLE m_hCloseEvent;
	HANDLE m_hCloseWithoutFadeEvent;
	HBITMAP m_hbmBackground;
	HPALETTE m_hPalette;
	BYTE m_byAlpha;
	const CConfiguration& m_oConfig;
	HWND m_hwnd;
	BOOL m_bIsUpgradeInProgress;
	BOOL m_bProductUpgraded;
	DWORD m_dwDBToolExitCode;
	std::string_t m_sDBToolLogFile;

	typedef std::vector<CSharedPtr<CHyperlink>> HyperlinkContainer;
	HyperlinkContainer m_hyperlinks;
	CSharedPtr<CHyperlink> m_spStatusLink;
	CTextAttributes m_oTextAttributes;
	DWORD m_dwNextUpgradeState;
	UINT m_iTimer;
	DWORD m_dwWaitForServiceReadyMutexTickCount;
	std::string_t m_sDatabasePathDir;
	std::string_t m_sUpdateBackupsDatabasePathDir;
	std::string_t m_sPriorVersion;
	std::string_t m_sPriorVersionConnectorServiceStartName;

	// Disallow copying and assignment
	CBrowserWindow(const CBrowserWindow&);
	CBrowserWindow& operator=(const CBrowserWindow&);

	BOOL IsProductInstalled(LPCTSTR tszProductCode);

	static DWORD DoCreateProcess(std::ofstream_t& oLogStream, LPCTSTR tszCommand, bool bCreateNoWindow, DWORD& dwExitCode);
};
