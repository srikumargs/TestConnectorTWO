#include "StdAfx.h"
#include "resource.h"
#include "BrowserWindow.h"
#include "Hyperlink.h"
#include <shlobj.h>
#include <shlwapi.h>
#include <msi.h>
#include "..\ProductInfo\ProductInfo.h"
#include "..\LinkedSource\VariablesHandler.h"
#include "..\LinkedSource\ServiceUtils.h"
#include "..\LinkedSource\FileUtils.h"
#include "..\LinkedSource\WebUtils.h"
#include "..\LinkedSource\RunningProcessManager.h"

CBrowserWindow::CBrowserWindow(HINSTANCE hInstance, const CConfiguration& oConfig) 
	: m_oConfig(oConfig),
	m_hInstance(NULL),
	m_dwFadeoutEnd(0),
	m_hCloseEvent(NULL),
	m_hCloseWithoutFadeEvent(NULL),
	m_hbmBackground(NULL),
	m_byAlpha(0),
	m_hwnd(NULL),
	m_bIsUpgradeInProgress(FALSE),
	m_dwNextUpgradeState(0),
	m_iTimer(0),
	m_dwWaitForServiceReadyMutexTickCount(0)
{
	m_sBrowserWindowClass = Format(_T("SageUpgradeWindow-%d"), GetCurrentProcessId());
	m_hInstance = hInstance;
	m_dwFadeoutEnd=0;
}

void CBrowserWindow::SetBackgroundImage(HWND hwnd, HBITMAP hbmp)
{
	// get the size of the bitmap
	BITMAP bm;
	POINT ptZero = { 0 };

	GetObject(hbmp, sizeof(bm), &bm);

	int cyCaption = m_oConfig.GetShowWindowCaption() ? ::GetSystemMetrics(SM_CYCAPTION) : 0;
	int cxFixedFrame = m_oConfig.GetShowWindowCaption() ? ::GetSystemMetrics(SM_CXFIXEDFRAME) : 0;
	int cyFixedFrame = m_oConfig.GetShowWindowCaption() ? ::GetSystemMetrics(SM_CYFIXEDFRAME) : 0;

	SIZE size = { bm.bmWidth + (2*cxFixedFrame), bm.bmHeight + cyCaption + (2*cyFixedFrame)};

	// get the primary monitor's info
	HMONITOR hmonPrimary = MonitorFromPoint(ptZero, MONITOR_DEFAULTTOPRIMARY);
	MONITORINFO monitorinfo = { 0 };
	monitorinfo.cbSize = sizeof(monitorinfo);
	GetMonitorInfo(hmonPrimary, &monitorinfo);

	// center the window in the middle of the primary work area
	const RECT & rcWork = monitorinfo.rcWork;
	POINT ptOrigin;

	ptOrigin.x = rcWork.left + (rcWork.right - rcWork.left - size.cx) / 2;
	ptOrigin.y = rcWork.top + (rcWork.bottom - rcWork.top - size.cy) / 2;

	// use the source image's alpha channel for blending
	m_byAlpha = 0xFF;

	::SetWindowPos(hwnd ,       // handle to window
		0,  // placement-order handle
		ptOrigin.x,     // horizontal position
		ptOrigin.y,      // vertical position
		size.cx,  // width
		size.cy, // height
		SWP_SHOWWINDOW); // window-positioning options);
}

HWND CBrowserWindow::CreateBrowserWindow()
{
	DWORD dwAdditionalWindowStyles = WS_POPUP;
	if(m_oConfig.GetShowWindowCaption())
	{
		dwAdditionalWindowStyles = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_MINIMIZEBOX;
	}
	HWND hwndOwner = CreateWindow(m_sBrowserWindowClass.c_str(), NULL, dwAdditionalWindowStyles , 0, 0, 0, 0, NULL, NULL, m_hInstance, NULL);
	return CreateWindowEx(WS_EX_LAYERED | WS_EX_APPWINDOW, m_sBrowserWindowClass.c_str(), m_oConfig.GetWindowCaption().c_str(), dwAdditionalWindowStyles | WS_VISIBLE | WS_BORDER, 0, 0, 0, 0, hwndOwner, NULL, m_hInstance, NULL);
}

void CBrowserWindow::RegisterWindowClass()
{
	WNDCLASS wc = { 0 };
	wc.lpfnWndProc = &CBrowserWindow::WindowProc;
	wc.hInstance = m_hInstance;
	wc.hIcon = LoadIcon(m_hInstance, MAKEINTRESOURCE(IDI_SageUpgrade));
	wc.hCursor = LoadCursor(NULL, IDC_ARROW); 
	wc.lpszClassName = m_sBrowserWindowClass.c_str();

	RegisterClass(&wc);
}

void CBrowserWindow::UnregisterWindowClass() 
{
	UnregisterClass(m_sBrowserWindowClass.c_str(), m_hInstance);
}

void CBrowserWindow::FadeWindowOut(HWND hWnd)
{
	DWORD dtNow = GetTickCount();
	if (dtNow >= m_dwFadeoutEnd) 
	{
		PostQuitMessage(0);
	} 
	else
	{ 
		double fade = ((double)m_dwFadeoutEnd - (double)dtNow) / (double)m_oConfig.GetExitFadeMS();
		m_byAlpha = (byte)(255 * fade);
		SetLayeredWindowAttributes(hWnd, 0, m_byAlpha, LWA_ALPHA);
	} 
}

inline DWORD CBrowserWindow::PumpMsgWaitForMultipleObjects(HWND hWnd, DWORD nCount, LPHANDLE pHandles, DWORD dwMilliseconds)
{
	const DWORD dwStartTickCount = ::GetTickCount();
	for (;;)
	{
		// calculate timeout
		const DWORD dwElapsed = GetTickCount() - dwStartTickCount;
		const DWORD dwTimeout = dwMilliseconds == INFINITE ? INFINITE :dwElapsed < dwMilliseconds ? dwMilliseconds - dwElapsed : 0;

		// wait for a handle to be signaled or a message
		const DWORD dwWaitResult = MsgWaitForMultipleObjects(nCount, pHandles, FALSE, dwTimeout, QS_ALLINPUT);
		if (dwWaitResult == WAIT_OBJECT_0 + nCount)
		{
			// pump messages
			MSG msg;
			while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE) != FALSE)
			{
				switch(msg.message)
				{
				case WM_QUIT:
					// repost quit message and return
					PostQuitMessage((int) msg.wParam);
					return WAIT_OBJECT_0 + nCount;
					break;
				case WM_KEYDOWN:
					if(msg.wParam == VK_ESCAPE)
					{
						SetEvent(m_hCloseEvent);
					}
					break;
				}

				// dispatch thread message
				TranslateMessage(&msg);
				DispatchMessage(&msg);
			}
		}
		else
		{
			// Check fade event.  If the fade event is not set then we simply need to exit.  
			// if the fade event is set then we need to fade out  
			const DWORD dwWaitResult = MsgWaitForMultipleObjects(1, &pHandles[0], FALSE, 0, QS_ALLINPUT);
			if (dwWaitResult == WAIT_OBJECT_0)
			{
				MSG msg;
				// timeout on actual wait or any other object
				SetTimer(hWnd, 1, 30, NULL);
				m_dwFadeoutEnd = GetTickCount() + m_oConfig.GetExitFadeMS();
				BOOL bRet;

				while( (bRet = GetMessage( &msg, hWnd, 0, 0 )) != 0)
				{ 
					if (bRet == -1)
					{
						// handle the error and possibly exit
					}
					else
					{
						if (msg.message==WM_TIMER && msg.wParam == 1) 
						{
							FadeWindowOut(hWnd);
						}
						TranslateMessage(&msg); 
						DispatchMessage(&msg); 
					}
				}
			}
			return dwWaitResult;
		}
	}
}

BOOL LoadBitmapFromBMPFile(LPCTSTR szFileName, HBITMAP *phBitmap, HPALETTE *phPalette)
{
	BITMAP  bm;

	*phBitmap = NULL;
	*phPalette = NULL;

	// Use LoadImage() to get the image loaded into a DIBSection
	*phBitmap = (HBITMAP)LoadImage( NULL, szFileName, IMAGE_BITMAP, 0, 0, LR_CREATEDIBSECTION | LR_DEFAULTSIZE | LR_LOADFROMFILE );
	if( *phBitmap == NULL )
	{
		return FALSE;
	}

	// Get the color depth of the DIBSection
	GetObject(*phBitmap, sizeof(BITMAP), &bm );

	// If the DIBSection is 256 color or less, it has a color table
	if( ( bm.bmBitsPixel * bm.bmPlanes ) <= 8 )
	{
		HDC           hMemDC;
		HBITMAP       hOldBitmap;
		RGBQUAD       rgb[256];
		LPLOGPALETTE  pLogPal;
		WORD          i;

		// Create a memory DC and select the DIBSection into it
		hMemDC = CreateCompatibleDC( NULL );
		hOldBitmap = (HBITMAP)SelectObject( hMemDC, *phBitmap );

		// Get the DIBSection's color table
		GetDIBColorTable( hMemDC, 0, 256, rgb );

		// Create a palette from the color tabl
		pLogPal = (LOGPALETTE *)malloc( sizeof(LOGPALETTE) + (256*sizeof(PALETTEENTRY)) );
		pLogPal->palVersion = 0x300;
		pLogPal->palNumEntries = 256;
		for(i=0 ; i<256 ; i++)
		{
			pLogPal->palPalEntry[i].peRed = rgb[i].rgbRed;
			pLogPal->palPalEntry[i].peGreen = rgb[i].rgbGreen;
			pLogPal->palPalEntry[i].peBlue = rgb[i].rgbBlue;
			pLogPal->palPalEntry[i].peFlags = 0;
		}
		*phPalette = CreatePalette( pLogPal );

		// Clean up
		free( pLogPal );
		SelectObject( hMemDC, hOldBitmap );
		DeleteDC( hMemDC );
	}
	else   // It has no color table, so use a halftone palette
	{
		HDC    hRefDC;
		hRefDC = GetDC( NULL );
		*phPalette = CreateHalftonePalette( hRefDC );
		ReleaseDC( NULL, hRefDC );
	}

	return TRUE;
}

void CBrowserWindow::Show()
{
	::CoInitialize(0);

	// create the named close browser window event, making sure we're the first process to create it
	SetLastError(ERROR_SUCCESS);

	std::string_t sEventName = Format(_T("SageUpgrade-CloseEvent-%d"), GetCurrentProcessId());
	m_oConfig.GetLogStream() << "Creating event '" << sEventName << "'" << std::endl;
	m_hCloseEvent = CreateEvent(NULL, TRUE, FALSE, sEventName.c_str());
	if (GetLastError() == ERROR_ALREADY_EXISTS)
	{
		ExitProcess(0);
	}

	sEventName = Format(_T("SageUpgrade-CloseWithoutFadeEvent-%d"), GetCurrentProcessId());
	m_oConfig.GetLogStream() << "Creating event '" << sEventName << "'" << std::endl;
	m_hCloseWithoutFadeEvent = CreateEvent(NULL, TRUE, FALSE, sEventName.c_str());
	if (GetLastError() == ERROR_ALREADY_EXISTS)
	{
		ExitProcess(0);
	}

	m_oConfig.GetLogStream() << "Loading image ..." << std::endl;
	if(FALSE == LoadBitmapFromBMPFile(m_oConfig.GetImageFilePath().c_str(), &m_hbmBackground, &m_hPalette))
	{
		ExitProcess(0);
	}

	RegisterWindowClass();

	if (m_hbmBackground != NULL)
	{
		m_hwnd = CreateBrowserWindow();
		::SetWindowLong(m_hwnd, GWL_USERDATA, (LONG)this);
		SetLayeredWindowAttributes(m_hwnd, 0, 255, LWA_ALPHA);
		SetBackgroundImage(m_hwnd, m_hbmBackground);
		SetupLinkedText(m_hwnd);
	}

	if (m_hwnd != NULL) 
	{
		if(m_oConfig.GetAutoUpgrade())
		{
			AdvanceUpgradeState(WM_UPGRADE_STARTING, _T("Starting update..."));
		}
		HANDLE waitHandles[2] = { m_hCloseEvent, m_hCloseWithoutFadeEvent };
		PumpMsgWaitForMultipleObjects(m_hwnd, 2, &waitHandles[0], INFINITE);
	}

	DeleteObject(m_hbmBackground);
	DeleteObject(m_hPalette);

	CloseHandle(m_hCloseEvent);
	CloseHandle(m_hCloseWithoutFadeEvent);

	UnregisterWindowClass();

	::CoUninitialize();
}

bool PathIsExcludedFromCopyOperation(const std::string_t& value)
{ return (EndsWith(value, _T("\\Documents")) || EndsWith(value, _T("\\Backups")) || EndsWith(value, _T("\\Baseline"))); }

void UpdateOnePointTwoOrOlderInstanceConfigXml(std::ofstream_t& oLogStream, const std::string_t& sInstallPath)
{			
	// existing 1.2 install users _may_ have alterred their InstanceConfig.xml based on local group policy (e.g.,
	// StartingPort is the most likely culprit).  Migrate previous StartingPort value to the latest
	// InstanceConfig.xml

	TCHAR tszPath[MAX_PATH];
	TCHAR tszTempPath[MAX_PATH];
	TCHAR tszDefaultsPath[MAX_PATH];
	::PathCombine(tszPath, sInstallPath.c_str(), _T("Sage.CRE.HostingFramework.Service-InstanceConfig.xml"));
	::PathCombine(tszTempPath, sInstallPath.c_str(), _T("Sage.CRE.HostingFramework.Service-InstanceConfig.xml.tmp"));
	::PathCombine(tszDefaultsPath, sInstallPath.c_str(), _T("Sage.CRE.HostingFramework.Service-InstanceConfigDefaults.xml"));
	if(FileExists(oLogStream, tszPath))
	{
		SageCopyFile(oLogStream, tszPath, tszTempPath);

		// read the old StartingPort value
		std::string_t sStartingPortFromOldFile;
		std::ifstream_t temp(tszTempPath);
		if(temp.is_open())
		{
			while(temp.good())
			{								
				std::string_t sLine;
				if(std::getline(temp, sLine))
				{							
					std::tr1::cmatch_t match;
					std::tr1::regex_t regex(_T("(<StartingPort>)(.*)(</StartingPort>)"));
					std::tr1::regex_search(sLine.c_str(), match, regex);
					if(match.size() > 0)
					{
						sStartingPortFromOldFile = match[2];
					}
				}		
			}

			temp.close(); 

			DeleteFile(oLogStream, tszTempPath);
		}


		// recreate the InstanceConfig.xml using the latest InstanceConfigDefaults.xml + the StartingPort read above
		std::ifstream_t in;
		in.open(tszDefaultsPath, std::ios::in);

		std::ofstream_t out;
		out.open(tszPath, std::ios::out | std::ios::trunc);
		if(in.is_open() && out.is_open())
		{
			while(in.good())
			{
				std::string_t sLine;
				if(std::getline(in, sLine))
				{
					sLine = Replace(sLine, _T("<StartingPort>48810</StartingPort>"), Format(_T("<StartingPort>%s</StartingPort>"), sStartingPortFromOldFile.c_str()));
					sLine = Replace(sLine, _T("<StartingPort>48800</StartingPort>"), Format(_T("<StartingPort>%s</StartingPort>"), sStartingPortFromOldFile.c_str()));
				}

				out << sLine << std::endl;
			}

			in.close(); 
			out.close(); 
		}
	}
}

BOOL VersionIsOnePointTwoOrOlder(const std::string_t& sVersion)
{
	return 
		(StartsWith(sVersion, _T("0.")) || 
		StartsWith(sVersion, _T("1.0")) || 
		StartsWith(sVersion, _T("1.1")) || 
		StartsWith(sVersion, _T("1.2")));
}

void ParseVersion(int result[4], const std::string_t& input)
{
    std::istringstream_t parser(input);
    parser >> result[0];
    for(int idx = 1; idx < 4; idx++)
    {
        parser.get(); //Skip period
        parser >> result[idx];
    }
}

bool LessThanVersion(const std::string_t& a,const std::string_t& b)
{
    int parsedA[4], parsedB[4];
    ParseVersion(parsedA, a);
    ParseVersion(parsedB, b);
    return std::lexicographical_compare(parsedA, parsedA + 4, parsedB, parsedB + 4);
}

LRESULT CALLBACK CBrowserWindow::WindowProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	CBrowserWindow* pBrowserWindow = (CBrowserWindow*)GetWindowLong(hwnd, GWL_USERDATA);

	switch(uMsg)
	{
	case WM_PAINT:
		{
			HDC hDC;
			BITMAP bm;
			PAINTSTRUCT ps = {0};

			hDC = BeginPaint( hwnd, &ps );

			GetObject(pBrowserWindow->m_hbmBackground, sizeof(bm), &bm);
			HDC hMemDC = CreateCompatibleDC(hDC);
			HBITMAP hOldBitmap = (HBITMAP)SelectObject(hMemDC, pBrowserWindow->m_hbmBackground);
			HPALETTE hOldPalette = SelectPalette( hDC, pBrowserWindow->m_hPalette, FALSE );
			RealizePalette( hDC );

			BitBlt(hDC, 0, 0, bm.bmWidth, bm.bmHeight, hMemDC, 0, 0, SRCCOPY);

			SelectObject(hMemDC, hOldBitmap);
			SelectPalette( hDC, hOldPalette, FALSE );
			DeleteDC(hMemDC);

			EndPaint( hwnd, &ps );
		}
		return 0;
		break;

	case WM_CLOSE:
		SetEvent(pBrowserWindow->m_hCloseEvent);
		return 0;
		break;

	case WM_UPGRADE_STARTING:
		{
			pBrowserWindow->m_bIsUpgradeInProgress = TRUE;
			pBrowserWindow->m_bProductUpgraded = FALSE;
			
			// Upgrades from 1.2 or older:  explicitly look at the old ProductVersion location first
			CVariablesHandler variablesHandler;
			pBrowserWindow->m_sPriorVersion = variablesHandler.ExpandAllVariables(_T("$(Environment::Registry::HKLM\\Software\\Sage\\ConstructionAnywhere\\STO\\ProductVersion)"));
			
			// Now look at the current ProductVersion location
			if(pBrowserWindow->m_sPriorVersion.length() == 0)
			{
				pBrowserWindow->m_sPriorVersion = variablesHandler.ExpandAllVariables(GetCurrentlyInstalledProductVersionExpression());
			}
			
			pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_STOP_MONITOR_SERVICE, _T("Stopping monitor service..."));
		}
		return 0;
		break;

	case WM_UPGRADE_STOP_MONITOR_SERVICE:
		{
			if(VersionIsOnePointTwoOrOlder(pBrowserWindow->m_sPriorVersion))
			{
				// Upgrades from 1.2 or older:  explicitly stop the STO Monitor Service; this is the old service name
				StopWindowsService(pBrowserWindow->m_oConfig.GetLogStream(), _T("Sage.CloudConnector.STO.Service.Monitor.1.0"));
			}
			
			StopWindowsService(pBrowserWindow->m_oConfig.GetLogStream(), GetMonitorHostingFxServiceName());
			pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_WAIT_MONITOR_SERVICE_NOT_READY, _T("Waiting for monitor service..."));
		}
		return 0;
		break;

	case WM_UPGRADE_WAIT_MONITOR_SERVICE_NOT_READY:
		{
			if(pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount == 0)
			{
				pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount = GetTickCount();
			}
			
			BOOL bOldMonitorServiceNotReady = TRUE;
			if(VersionIsOnePointTwoOrOlder(pBrowserWindow->m_sPriorVersion))
			{
				// Upgrades from 1.2 or older:  explicitly stop the STO Monitor Service; this is the old service name
				bOldMonitorServiceNotReady = WaitForServiceMutexToBeReleased(pBrowserWindow->m_oConfig.GetLogStream(), _T("Global\\__Sage.CloudConnector.STO.Service.Monitor.1.0_READY"), pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount, 120000);
			}
			
			BOOL bMonitorServiceNotReady = WaitForServiceMutexToBeReleased(pBrowserWindow->m_oConfig.GetLogStream(), GetMonitorHostingFxServiceReadyMutexName(), pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount, 120000);
			
			// FIXME: once we are assured that we are at least upgrading from 1.5, then the above logic should be changed to check the ServiceProcessRunningMutexName instead

			if(bOldMonitorServiceNotReady &&
				bMonitorServiceNotReady)
			{
				pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount = 0;
				pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_STOP_CONNECTOR_SERVICE, _T("Stopping connector service..."));
			}
			else
			{
				pBrowserWindow->DoUpgradeStateDelayed(1000);
			}
		}
		return 0;
		break;

	case WM_UPGRADE_STOP_CONNECTOR_SERVICE:
		{
			if(VersionIsOnePointTwoOrOlder(pBrowserWindow->m_sPriorVersion))
			{
				// Upgrades from 1.2 or older:  explicitly stop the STO Monitor Service; this is the old service name
				StopWindowsService(pBrowserWindow->m_oConfig.GetLogStream(), _T("Sage.CloudConnector.STO.Service.1.0"));
			}
			
			StopWindowsService(pBrowserWindow->m_oConfig.GetLogStream(), GetConnectorHostingFxServiceName());
			pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_WAIT_CONNECTOR_SERVICE_NOT_READY, _T("Waiting for connector service..."));
		}
		return 0;
		break;

	case WM_UPGRADE_WAIT_CONNECTOR_SERVICE_NOT_READY:
		{
			if(pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount == 0)
			{
				pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount = GetTickCount();
			}
			
			BOOL bOldConnectorServiceNotReady = TRUE;
			if(VersionIsOnePointTwoOrOlder(pBrowserWindow->m_sPriorVersion))
			{
				// Upgrades from 1.2 or older:  explicitly stop the STO Service; this is the old service name
				bOldConnectorServiceNotReady = WaitForServiceMutexToBeReleased(pBrowserWindow->m_oConfig.GetLogStream(),  _T("Global\\__Sage.CloudConnector.STO.Service.1.0_READY"), pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount, 120000);
			}
			
			BOOL bConnectorServiceNotReady = WaitForServiceMutexToBeReleased(pBrowserWindow->m_oConfig.GetLogStream(), GetConnectorHostingFxServiceReadyMutexName(), pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount, 120000);
			
			// FIXME: once we are assured that we are at least upgrading from 1.5, then the above logic should be changed to check the ServiceProcessRunningMutexName instead

			if(bOldConnectorServiceNotReady &&
				bConnectorServiceNotReady)
			{
				pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount = 0;
				pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_PREPARE_PRODUCT, _T("Preparing for update..."));
			}
			else
			{
				pBrowserWindow->DoUpgradeStateDelayed(1000);
			}
		}
		return 0;
		break;

	case WM_UPGRADE_PREPARE_PRODUCT:
		{
			BOOL bOldProductFound = IsOldProductInstalled(pBrowserWindow->m_oConfig.GetLogStream());
			if(bOldProductFound)
			{
				pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_PREPARE_STOP_RUNNING_PROCS, _T("Checking for running processes..."));
			}
			else
			{
				pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_FINISHING, _T("Finishing...."));
			}
		}
		return 0;
		break;
	case WM_UPGRADE_PREPARE_STOP_RUNNING_PROCS:
		{
			CVariablesHandler variablesHandler;
			std::string_t sInstallPathDir;
			if(VersionIsOnePointTwoOrOlder(pBrowserWindow->m_sPriorVersion))
			{
				// Upgrades from 1.2 or older:  explicitly look at the old InstallPath
				sInstallPathDir = variablesHandler.ExpandAllVariables(_T("$(Environment::Registry::HKLM\\Software\\Sage\\ConstructionAnywhere\\STO\\InstallPath)"));
				if(sInstallPathDir.length() >0)
				{
					CRunningProcessManager manager;
					manager.StopRunningProcesses(pBrowserWindow->m_oConfig.GetLogStream(), sInstallPathDir);
				}
			}

			sInstallPathDir = variablesHandler.ExpandAllVariables(GetCurrentlyInstalledProductInstallPathExpression());
			if(sInstallPathDir.length() >0)
			{
				CRunningProcessManager manager;
				manager.StopRunningProcesses(pBrowserWindow->m_oConfig.GetLogStream(), sInstallPathDir);
			}

			pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_PREPARE_SQL_CE_40_SP1, _T("Inspecting SQL Server Compact 4.0..."));
		}
		return 0;
		break;

	case WM_UPGRADE_PREPARE_SQL_CE_40_SP1:
		{
			// logic stolen from package.xml (bootstrapper package description) in generic bootstrapper
			CVariablesHandler variablesHandler;
			std::string_t sEnuInst = variablesHandler.ExpandAllVariables(_T("$(Environment::Registry::HKLM\\SOFTWARE\\Microsoft\\Microsoft SQL Server Compact Edition\\v4.0\\ENU\\DesktopRuntimeVersion)"));
			
			if(sEnuInst.length() > 0 && (sEnuInst == _T("4.0.8876.1") || LessThanVersion(_T("4.0.8876.1"), sEnuInst)))
			{
				pBrowserWindow->m_oConfig.GetLogStream() << _T("SQL CE 4.0 SP1 or newer is already installed.") << std::endl;
				pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_BACKUP_DATABASE, _T("Backing up database..."));
			}
			else
			{
				pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_SQL_CE_40_SP1, _T("Updating SQL Server Compact 4.0 SP1..."));
			}
		}
		return 0;
		break;

	case WM_UPGRADE_SQL_CE_40_SP1:
		{					
			CVariablesHandler variablesHandler;
			std::string_t sProcessorArchitecture = variablesHandler.ExpandAllVariables(_T("$(Environment::System::PROCESSOR_ARCHITECTURE)"));
			std::string_t sProcessorArchitectureWow6432 = variablesHandler.ExpandAllVariables(_T("$(Environment::System::PROCESSOR_ARCHITEW6432)"));
			if(sProcessorArchitecture == _T("AMD64") || sProcessorArchitectureWow6432 == _T("AMD64"))
			{
				// OS is 64 bit
				std::string_t sCommand(_T("SSCERuntime_x64-ENU.exe /i /q"));
				DWORD dwExitCode;
				DoCreateProcess(pBrowserWindow->m_oConfig.GetLogStream(), sCommand.c_str(), false, dwExitCode);
			}
			else
			{
				// OS is 32 bit
				std::string_t sCommand(_T("SSCERuntime_x86-ENU.exe /i /q"));
				DWORD dwExitCode;
				DoCreateProcess(pBrowserWindow->m_oConfig.GetLogStream(), sCommand.c_str(), false, dwExitCode);
			}

			pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_BACKUP_DATABASE, _T("Backing up database..."));
		}
		return 0;
		break;


	case WM_UPGRADE_BACKUP_DATABASE:
		{
			CVariablesHandler variablesHandler;

			std::string_t sOldInstanceAppDataFolder;
			std::string_t sNewInstanceAppDataFolder;
			if(VersionIsOnePointTwoOrOlder(pBrowserWindow->m_sPriorVersion))
			{
				// Upgrades from 1.2 or older:  explicitly look at the old AppData Folder and move it to the new location
				sOldInstanceAppDataFolder = variablesHandler.ExpandAllVariables(_T("$(Environment::Folder::COMMON_APPDATA)\\Sage\\CRE\\HostingFramework\\Sage.CloudConnector.STO.Service.1.0"));
				sNewInstanceAppDataFolder = variablesHandler.ExpandAllVariables(GetConnectorServiceInstanceAppDataFolderExpression());
				if(DirectoryExists(pBrowserWindow->m_oConfig.GetLogStream(), sOldInstanceAppDataFolder.c_str()))
				{
					MoveDirectoryRecursive(pBrowserWindow->m_oConfig.GetLogStream(), sOldInstanceAppDataFolder.c_str(), sNewInstanceAppDataFolder.c_str());
				}
			}

			// Now look at what is already in our instance app data folder and back it up
			pBrowserWindow->m_sDatabasePathDir = sNewInstanceAppDataFolder;
			if(pBrowserWindow->m_sDatabasePathDir.length() >0)
			{
				pBrowserWindow->m_sUpdateBackupsDatabasePathDir = pBrowserWindow->m_sDatabasePathDir;
				pBrowserWindow->m_sUpdateBackupsDatabasePathDir += _T("\\Backups\\");

				TCHAR tszDateTimeStamp[MAX_PATH];
				::SecureZeroMemory(tszDateTimeStamp, sizeof(tszDateTimeStamp));
				time_t lt;
				time (&lt);
				tm mytm;
				localtime_s (&mytm, &lt); // get current time
				_tcsftime(tszDateTimeStamp, MAX_PATH, _T("%Y%m%d.%H%M%S"), &mytm); // format date

				pBrowserWindow->m_sUpdateBackupsDatabasePathDir += tszDateTimeStamp;
				pBrowserWindow->m_sUpdateBackupsDatabasePathDir += _T("_");
				pBrowserWindow->m_sUpdateBackupsDatabasePathDir += pBrowserWindow->m_sPriorVersion;
				pBrowserWindow->m_sUpdateBackupsDatabasePathDir += _T("_Update");

				std::list<std::string_t> fileNames = FindFiles(pBrowserWindow->m_oConfig.GetLogStream(), (pBrowserWindow->m_sDatabasePathDir).c_str(), _T("*.*"));
				fileNames.remove_if(PathIsExcludedFromCopyOperation);
				CopyFiles(pBrowserWindow->m_oConfig.GetLogStream(), fileNames, pBrowserWindow->m_sUpdateBackupsDatabasePathDir);
				DeleteFiles(pBrowserWindow->m_oConfig.GetLogStream(), fileNames);
			}

			pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_PRODUCT, _T("Updating product..."));
		}
		return 0;
		break;


	case WM_UPGRADE_PRODUCT:
		{
			TCHAR tszPath[MAX_PATH];
			TCHAR tszLogPath[MAX_PATH];
			GetTempPath(MAX_PATH, tszPath);
			GetTempFileName(tszPath, _T("SU-"), 0, tszLogPath);
			std::string_t sCommand(_T("msiexec.exe /i SageConnector.msi"));
			sCommand += _T(" REINSTALLMODE=vomus REINSTALL=ALL ");
			sCommand += _T(" /l*vx \"");
			sCommand += tszLogPath;
			sCommand += _T("\"");

			DWORD dwExitCode;
			DoCreateProcess(pBrowserWindow->m_oConfig.GetLogStream(), sCommand.c_str(), false, dwExitCode);

			// If the upgrade failed the old server will still be installed.
			if (!IsOldProductInstalled(pBrowserWindow->m_oConfig.GetLogStream()))
			{
				pBrowserWindow->m_bProductUpgraded = TRUE;
				pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_RECONFIGURE_CONNECTOR_SERVICE, _T("Reconfiguring connector service..."));
			}
			else
			{
				// Possible that the user cancelled the MSI upgrade
				// restore backed-up files back to their operational spots
				std::list<std::string_t> fileNames = FindFiles(pBrowserWindow->m_oConfig.GetLogStream(), (pBrowserWindow->m_sUpdateBackupsDatabasePathDir).c_str(), _T("*.*"));
				CopyFiles(pBrowserWindow->m_oConfig.GetLogStream(), fileNames, pBrowserWindow->m_sDatabasePathDir);

				pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_FINISHING, _T("Finishing...."));
			}
		}
		return 0;
		break;

	case WM_UPGRADE_RECONFIGURE_CONNECTOR_SERVICE:
		{
			// Somewhere before v1.4 a bug was introduced in the install that will make the InstanceConfigDefaults.xml disappear during
			// an upgrade install.  It is hoped that this bug has been fixed ... but this hack is necessary, for now, to make certain
			// the file doesn't go missing after the msi upgrade from 1.2 or older is completed.  At some point in the future (when upgrade
            // from 1.2 is no longer supported, we should remove this fix (and the code in the Install.fbp7 that copies the file into the
            // SFX) and retest.
			TCHAR tszPath[MAX_PATH];
			CVariablesHandler variablesHandler;
			std::string_t sInstallPathDir = variablesHandler.ExpandAllVariables(GetCurrentlyInstalledProductInstallPathExpression());

			::PathCombine(tszPath, sInstallPathDir.c_str(), _T("Sage.CRE.HostingFramework.Service-InstanceConfigDefaults.xml"));
			std::string_t sSourceFile = pBrowserWindow->m_oConfig.ExpandToFullPath(_T("Sage.CRE.HostingFramework.Service-InstanceConfigDefaults.xml"));
			if(FileExists(pBrowserWindow->m_oConfig.GetLogStream(), sSourceFile.c_str()))
			{
				SageCopyFile(pBrowserWindow->m_oConfig.GetLogStream(), sSourceFile.c_str(), tszPath);
			}


			if(VersionIsOnePointTwoOrOlder(pBrowserWindow->m_sPriorVersion))
			{
				// Upgrades from 1.2 or older:  delete the old service and force update the service name and service description to their new values
				std::string_t sServiceName = _T("Sage.CloudConnector.STO.Service.1.0");
				if(ServiceExists(pBrowserWindow->m_oConfig.GetLogStream(), sServiceName.c_str()))
				{
                    // store the old service account name so that we can prefill with it later on when re-prompting
					std::string_t sServiceStartName;
					if(GetServiceStartName(pBrowserWindow->m_oConfig.GetLogStream(), sServiceName.c_str(), sServiceStartName))
					{
						pBrowserWindow->m_oConfig.GetLogStream() << _T("GetServiceStartName '") << sServiceName << _T("' succeeded; result='") << sServiceStartName << _T("'.") << std::endl;
						pBrowserWindow->m_sPriorVersionConnectorServiceStartName = sServiceStartName;
					}

					DeleteWindowsService(pBrowserWindow->m_oConfig.GetLogStream(), sServiceName.c_str());
				}
				
				// Upgrades from 1.2 or older:  explicitly update the Connector service's InstanceConfig.xml (since it is not installed, but created on-demand)
				CVariablesHandler variablesHandler;
				std::string_t sInstallPath = variablesHandler.ExpandAllVariables(GetCurrentlyInstalledProductInstallPathExpression());
				UpdateOnePointTwoOrOlderInstanceConfigXml(pBrowserWindow->m_oConfig.GetLogStream(), sInstallPath);
			}

			pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_RECONFIGURE_MONITOR_SERVICE, _T("Reconfiguring monitor service..."));
			return 0;
		}
		break;
		
	case WM_UPGRADE_RECONFIGURE_MONITOR_SERVICE:
		{
			if(VersionIsOnePointTwoOrOlder(pBrowserWindow->m_sPriorVersion))
			{
				// Upgrades from 1.2 or older:  explicitly update the service name and service description to their new values
				std::string_t sServiceName = _T("Sage.CloudConnector.STO.Service.Monitor.1.0");
				if(ServiceExists(pBrowserWindow->m_oConfig.GetLogStream(), sServiceName.c_str()))
				{
					DeleteWindowsService(pBrowserWindow->m_oConfig.GetLogStream(), sServiceName.c_str());
				}
				
				// Upgrades from 1.2 or older:  explicitly update the Monitor service's InstanceConfig.xml (since it is not installed, but created on-demand)
				CVariablesHandler variablesHandler;
				std::string_t sInstallPath = variablesHandler.ExpandAllVariables(GetCurrentlyInstalledProductInstallPathExpression());
				sInstallPath += _T("Monitor\\Service\\");
				UpdateOnePointTwoOrOlderInstanceConfigXml(pBrowserWindow->m_oConfig.GetLogStream(), sInstallPath);
			}

			pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_PRIOR_DATABASE, _T("Updating database..."));
			return 0;
		}
		break;


	case WM_UPGRADE_PRIOR_DATABASE:
		{
			CVariablesHandler variablesHandler;

			std::string_t sInstallPathDir = variablesHandler.ExpandAllVariables(GetCurrentlyInstalledProductInstallPathExpression());
			if(sInstallPathDir.length() > 0)
			{
				std::string_t sCurrentVersion = variablesHandler.ExpandAllVariables(GetCurrentlyInstalledProductVersionExpression());
				if(pBrowserWindow->m_sDatabasePathDir.length() > 0)
				{
					TCHAR tszPath[MAX_PATH];
					TCHAR tszLogPath[MAX_PATH];
					GetTempPath(MAX_PATH, tszPath);
					GetTempFileName(tszPath, _T("SU-"), 0, tszLogPath);
					pBrowserWindow->m_sDBToolLogFile = tszLogPath;
					std::string_t sCommand = sInstallPathDir;
					sCommand += _T("DBTool.exe /silent /o:SchemaUpgradeAndDataMigrate");
					sCommand += _T(" /idd:\"");
					sCommand += pBrowserWindow->m_sDatabasePathDir;
					sCommand += _T("\" /pvbd:\"");
					sCommand += pBrowserWindow->m_sUpdateBackupsDatabasePathDir;
					sCommand += _T("\" /cv:");
					sCommand += sCurrentVersion;
					sCommand += _T(" /pv:");
					sCommand += pBrowserWindow->m_sPriorVersion;
					sCommand += _T(" /out:\"");
					sCommand += pBrowserWindow->m_sDBToolLogFile;
					sCommand += _T("\"");

					DoCreateProcess(pBrowserWindow->m_oConfig.GetLogStream(), sCommand.c_str(), true, pBrowserWindow->m_dwDBToolExitCode);
				}
			}

			pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_FINISHING, _T("Finishing...."));
		}
		return 0;
		break;

	case WM_UPGRADE_FINISHING:
		pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_START_CONNECTOR_SERVICE, _T("Starting connector service..."));
		return 0;
		break;
		
	case WM_UPGRADE_START_CONNECTOR_SERVICE:	
		{
			CVariablesHandler variables;
			std::string_t sInstallPath = variables.ExpandAllVariables(GetCurrentlyInstalledProductInstallPathExpression());

			pBrowserWindow->m_oConfig.GetLogStream() << GetCurrentlyInstalledProductInstallPathExpression() << _T(" is ") << sInstallPath << _T(".") << std::endl;

			std::string_t sCommand = sInstallPath;
			if(VersionIsOnePointTwoOrOlder(pBrowserWindow->m_sPriorVersion) && pBrowserWindow->m_sPriorVersionConnectorServiceStartName.length() > 0)
			{	
                // Upgrades from 1.2 or older:  invoke UI now to re-prompt for credentials for service
				sCommand += _T("SageConnector.exe /upgradeaccountselection /user:\"");
				sCommand += pBrowserWindow->m_sPriorVersionConnectorServiceStartName;
				sCommand += _T("\"");

				DWORD dwExitCode;
				DoCreateProcess(pBrowserWindow->m_oConfig.GetLogStream(), sCommand.c_str(), false, dwExitCode);
				if(dwExitCode != 0)
				{
					std::string_t  sMsg = _T("You will need to finish re-configuring the Connector service before using ");
					sMsg += GetBriefProductName();
					sMsg += _T(".  This can be completed later by re-launching the ");
					sMsg += GetBriefProductName();
					sMsg += _T(".");
					sMsg += _T("\r\n\r\nPreviously, you used '");
					sMsg += pBrowserWindow->m_sPriorVersionConnectorServiceStartName;
					sMsg += _T("' as your Connector service account");

					MessageBox(hwnd, sMsg.c_str(), _T("Service Configuration Incomplete"), MB_OK|MB_ICONINFORMATION);

					pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_START_MONITOR_SERVICE, _T("Starting monitor service..."));
				}
				else
				{
					pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_FINISHING_SUMMARY, _T("Summarizing..."));
				}
			}
			else
			{	
                // stopgap measure just in case (for some mysterious reason) the service isn't installed (e.g., the previous installation was damaged somehow)
				sCommand += _T("Sage.CRE.HostingFramework.Service.exe /install /silent");
				
				DWORD dwExitCode;
				DoCreateProcess(pBrowserWindow->m_oConfig.GetLogStream(), sCommand.c_str(), false, dwExitCode);

				StartWindowsService(pBrowserWindow->m_oConfig.GetLogStream(), GetConnectorHostingFxServiceName());

				pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_WAIT_CONNECTOR_SERVICE_READY, _T("Waiting for connector service..."));
			}
			return 0;
		}
		break;

	case WM_UPGRADE_WAIT_CONNECTOR_SERVICE_READY:	
		{
			if(pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount == 0)
			{
				pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount = GetTickCount();
			}
			
			if(WaitForServiceMutexToBeSet(pBrowserWindow->m_oConfig.GetLogStream(), GetConnectorHostingFxServiceReadyMutexName(), pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount, 120000))
			{
				pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount = 0;
				pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_START_MONITOR_SERVICE, _T("Starting monitor service..."));
			}
			else
			{
				pBrowserWindow->DoUpgradeStateDelayed(1000);
			}
		}
		return 0;
		break;

	case WM_UPGRADE_START_MONITOR_SERVICE:	
		{
			CVariablesHandler variables;
			std::string_t sInstallPath = variables.ExpandAllVariables(GetCurrentlyInstalledProductInstallPathExpression());

			pBrowserWindow->m_oConfig.GetLogStream() << GetCurrentlyInstalledProductInstallPathExpression() << _T(" is ") << sInstallPath << _T(".") << std::endl;

            // stopgap measure just in case (for some mysterious reason) the service isn't installed (e.g., the previous installation was damaged somehow)
			std::string_t sCommand = sInstallPath;
			sCommand += _T("Monitor\\Service\\Sage.CRE.HostingFramework.Service.exe /install /silent");

			DWORD dwExitCode;
			DoCreateProcess(pBrowserWindow->m_oConfig.GetLogStream(), sCommand.c_str(), false, dwExitCode);

			StartWindowsService(pBrowserWindow->m_oConfig.GetLogStream(), GetMonitorHostingFxServiceName());
			pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_WAIT_MONITOR_SERVICE_READY, _T("Waiting for monitor service..."));
			return 0;
		}
		break;

	case WM_UPGRADE_WAIT_MONITOR_SERVICE_READY:		
		{
			if(pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount == 0)
			{
				pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount = GetTickCount();
			}
			
			if(WaitForServiceMutexToBeSet(pBrowserWindow->m_oConfig.GetLogStream(), GetMonitorHostingFxServiceReadyMutexName(), pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount, 120000))
			{
				pBrowserWindow->m_dwWaitForServiceReadyMutexTickCount = 0;
				pBrowserWindow->AdvanceUpgradeState(WM_UPGRADE_FINISHING_SUMMARY, _T("Summarizing..."));
			}
			else
			{
				pBrowserWindow->DoUpgradeStateDelayed(1000);
			}
		}
		return 0;
		break;

	case WM_UPGRADE_FINISHING_SUMMARY:
		if(!pBrowserWindow->m_oConfig.GetAutoUpgrade())
		{
			std::string_t sProductName = GetBriefProductName();

			if(pBrowserWindow->m_bProductUpgraded)
			{
				if(VersionIsOnePointTwoOrOlder(pBrowserWindow->m_sPriorVersion))
				{
					// Upgrades from 1.2 or older:  explicitly look for and delete the at the old Sage.CloudConnector.STO.Service.Monitor.1.0 AppData Folder (MSI doesn't know about it)
					CVariablesHandler variablesHandler;
					std::string_t sOldInstanceAppDataFolder = variablesHandler.ExpandAllVariables(_T("$(Environment::Folder::COMMON_APPDATA)\\Sage\\CRE\\HostingFramework\\Sage.CloudConnector.STO.Service.Monitor.1.0"));
					DeleteDirectory(pBrowserWindow->m_oConfig.GetLogStream(), sOldInstanceAppDataFolder.c_str());
				}

				pBrowserWindow->ShowStatus(_T("Update completed."));
				std::string_t sMsg = sProductName;
				sMsg += _T(" update complete.");
				if(pBrowserWindow->m_dwDBToolExitCode == 2)
				{
					sMsg += _T("\r\n\r\nThe data migration did not complete. The previous data files have been backed up and replaced.");
					sMsg += _T("\r\n\r\nFor more information see the migration log: '");
					sMsg += pBrowserWindow->m_sDBToolLogFile;
					sMsg += _T("'.");
				}
				MessageBox(hwnd, sMsg.c_str(), _T("Update Complete"), MB_OK|MB_ICONINFORMATION);
			}
			else
			{
				pBrowserWindow->ShowStatus(_T("Update incomplete: nothing to do."));

				std::string_t sMsg = _T("The update cannot be completed because one of the following has occurred:\r\n\r\n\
• The installation process was aborted by the user. \r\n\r\n\
• All existing installations of ");
				sMsg += sProductName;
				sMsg += _T(" are current. In this situation, no action is needed. \r\n\r\n\
• No installation of ");
				sMsg += sProductName;
				sMsg += _T(" exists. In this situation, perform a full installation of ");
				sMsg += sProductName;
				sMsg += _T(" before performing the update. "); 

				MessageBox(hwnd, sMsg.c_str(), _T("Update Incomplete"), MB_OK | MB_ICONERROR);
			}

			pBrowserWindow->m_bIsUpgradeInProgress = FALSE;
		}
		else
		{
			pBrowserWindow->m_bIsUpgradeInProgress = FALSE;
			PostMessage(hwnd, WM_CLOSE, 0, 0);
		}
		return 0;
		break;

	case WM_TIMER:
		if(wParam == 2)
		{
			::KillTimer(hwnd, 2);
			pBrowserWindow->m_oConfig.GetLogStream() << _T("PostMessage(") << pBrowserWindow->m_dwNextUpgradeState << _T(").") << std::endl;
			::PostMessage(hwnd, pBrowserWindow->m_dwNextUpgradeState, 0, 0);
			return 0;
		}
		break;
	}

	return DefWindowProc(hwnd, uMsg, wParam, lParam);
}

BOOL CBrowserWindow::IsProductInstalled(LPCTSTR tszProductCode)
{
	BOOL bResult = FALSE;

	INSTALLSTATE installState = MsiQueryProductState(tszProductCode);
	m_oConfig.GetLogStream() << _T("MsiQueryProductState for '") << tszProductCode << _T("'; result is ") << installState << _T(".") << std::endl;
	if(installState == INSTALLSTATE_DEFAULT || installState == INSTALLSTATE_LOCAL)
	{
		bResult = TRUE;
	}

	return bResult;
}

void CBrowserWindow::AdvanceUpgradeState(DWORD dwState, LPCTSTR tstrStatus)
{
	ShowStatus(tstrStatus);
	m_dwNextUpgradeState = dwState;
	DoUpgradeStateDelayed(500);
	m_oConfig.GetLogStream() << _T("AdvanceUpgradeState: ") << dwState << _T(" '") << tstrStatus << _T("'.") << std::endl;
}

void CBrowserWindow::DoUpgradeStateDelayed(UINT uElapsed)
{
	m_iTimer = ::SetTimer(m_hwnd, 2, uElapsed, NULL);
}

void CBrowserWindow::SetupLinkedText(HWND hwnd)
{
	for(CConfiguration::DynamicTextInfoContainer::size_type i = 0 ; i < m_oConfig.GetDynamicTextInfo().size() ; i++)
	{
		const CDynamicTextInfo& oTextInfo = m_oConfig.GetDynamicTextInfo()[i];
		CHyperlink* pHyperlink = NULL;
		if(oTextInfo.GetCommandType() == ctNone)
		{
			pHyperlink = new CHyperlink(m_oConfig.GetLogStream(), oTextInfo.GetTextAttributes());
		}
		else
		{
			pHyperlink = new CHyperlink(m_oConfig.GetLogStream(), oTextInfo.GetTextAttributes(), oTextInfo.GetCommandType(), oTextInfo.GetCommand(), oTextInfo.GetCommandParameters(), this);
		}
		pHyperlink->Create(oTextInfo.GetX(), oTextInfo.GetY(), oTextInfo.GetText().c_str(), hwnd);
		if(oTextInfo.GetCommand().compare(std::string_t(_T("STATUS"))) != 0)
		{
			m_hyperlinks.push_back(CSharedPtr<CHyperlink>(pHyperlink));
		}
		else
		{
			m_oTextAttributes = oTextInfo.GetTextAttributes();
			m_spStatusLink = CSharedPtr<CHyperlink>(pHyperlink);
		}
	}
}

void CBrowserWindow::ShowStatus(LPCTSTR tszStatus)
{
	if(m_spStatusLink.get() != NULL)
	{
		::DestroyWindow(m_spStatusLink->GetHWND());
		m_spStatusLink = NULL;
	}
	CHyperlink* pStatusHyperlink = new CHyperlink(m_oConfig.GetLogStream(), 			m_oTextAttributes			);
	pStatusHyperlink->Create(61, 270, tszStatus, m_hwnd);
	::UpdateWindow(m_hwnd);

	m_spStatusLink = CSharedPtr<CHyperlink>(pStatusHyperlink);
}

DWORD CBrowserWindow::DoCreateProcess(std::ofstream_t& oLogStream, LPCTSTR tszCommand, bool bCreateNoWindow, DWORD& dwExitCode)
{
	DWORD dwResult = 0;
	dwExitCode = 0;

	PROCESS_INFORMATION pi;
	STARTUPINFO si;

	::ZeroMemory(&pi, sizeof(pi));
	::ZeroMemory(&si, sizeof(si));
	si.cb = sizeof(si);

	// Start the child process. 
	std::string_t sCommandLine;
	sCommandLine = tszCommand;
	oLogStream << _T("Performing 'CreateProcess' of '") << sCommandLine << _T("'.") << std::endl;
	dwResult = CreateProcess(NULL,                     // No module name (use command line). 
		const_cast<LPTSTR>(sCommandLine.c_str()),    // Command line. 
		NULL,                                        // Process handle not inheritable. 
		NULL,                                        // Thread handle not inheritable. 
		FALSE,                                       // Set handle inheritance to FALSE. 
		bCreateNoWindow ? CREATE_NO_WINDOW : 0,      // No creation flags. 
		NULL,                                        // Use parent's environment block. 
		NULL,										 // Use parent's starting directory. 
		&si,                                         // Pointer to STARTUPINFO structure.
		&pi);                                        // Pointer to PROCESS_INFORMATION structure.
	if(0 == dwResult)
	{
		dwResult = GetLastError();
		oLogStream << _T("'CreateProcess' failed. dwLastError = ") << dwResult << std::endl;
	}
	else
	{
		dwResult = 0;
		// Wait until child process exits.
		WaitForSingleObject(pi.hProcess, INFINITE);
		GetExitCodeProcess(pi.hProcess, &dwExitCode);
		oLogStream << _T("'CreateProcess' succeeded. dwExitCode = ") << dwExitCode << std::endl;

		// Close process and thread handles. 
		CloseHandle(pi.hThread);
		CloseHandle(pi.hProcess);
	}

	return dwResult;
}
