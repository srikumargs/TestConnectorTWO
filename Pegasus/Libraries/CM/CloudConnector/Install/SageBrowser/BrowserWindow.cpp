#include "StdAfx.h"
#include "resource.h"
#include "shlwapi.h"
#include "BrowserWindow.h"
#include "Hyperlink.h"

CBrowserWindow::CBrowserWindow(HINSTANCE hInstance, const CConfiguration& oConfig) 
: m_oConfig(oConfig),
m_hInstance(NULL),
m_dwFadeoutEnd(0),
m_hCloseEvent(NULL),
m_hCloseWithoutFadeEvent(NULL),
m_hbmBackground(NULL),
m_byAlpha(0)
{
	m_sBrowserWindowClass = Format(_T("SageBrowserWindow-%d"), GetCurrentProcessId());
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
	wc.hIcon = LoadIcon(m_hInstance, MAKEINTRESOURCE(IDI_SAGEBROWSER));
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
						if (msg.message==WM_TIMER) 
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

	std::string_t sEventName = Format(_T("SageBrowser-CloseEvent-%d"), GetCurrentProcessId());
	m_oConfig.GetLogStream() << "Creating event '" << sEventName << "'" << std::endl;
	m_hCloseEvent = CreateEvent(NULL, TRUE, FALSE, sEventName.c_str());
	if (GetLastError() == ERROR_ALREADY_EXISTS)
	{
		ExitProcess(0);
	}

	sEventName = Format(_T("SageBrowser-CloseWithoutFadeEvent-%d"), GetCurrentProcessId());
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

	HWND hwnd = NULL;
	if (m_hbmBackground != NULL)
	{
		hwnd = CreateBrowserWindow();
		::SetWindowLong(hwnd, GWL_USERDATA, (LONG)this);
		SetLayeredWindowAttributes(hwnd, 0, 255, LWA_ALPHA);
		SetBackgroundImage(hwnd, m_hbmBackground);
		SetupLinkedText(hwnd);
	}

	if (hwnd != NULL) 
	{
		HANDLE waitHandles[2] = { m_hCloseEvent, m_hCloseWithoutFadeEvent };
		PumpMsgWaitForMultipleObjects(hwnd, 2, &waitHandles[0], INFINITE);
	}

	DeleteObject(m_hbmBackground);
	DeleteObject(m_hPalette);

	CloseHandle(m_hCloseEvent);
	CloseHandle(m_hCloseWithoutFadeEvent);

	UnregisterWindowClass();

	::CoUninitialize();
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
	}

	return DefWindowProc(hwnd, uMsg, wParam, lParam);
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
			pHyperlink = new CHyperlink(m_oConfig.GetLogStream(), oTextInfo.GetTextAttributes(), oTextInfo.GetCommandType(), oTextInfo.GetCommand(), oTextInfo.GetCommandParameters());
		}
		pHyperlink->Create(oTextInfo.GetX(), oTextInfo.GetY(), oTextInfo.GetText().c_str(), hwnd);
		m_hyperlinks.push_back(CSharedPtr<CHyperlink>(pHyperlink));
	}
}