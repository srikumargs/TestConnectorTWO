#include "stdafx.h"
#include "Hyperlink.h"
#include "shellapi.h"
#include "windowsx.h"
#include "shlwapi.h"

#define STATIC_HYPER_WINDOW_CLASS _T("STATIC_HYPER")

CHyperlink::StaticInitializer::StaticInitializer()
{
	WNDCLASS hc;
	hc.style = 0;
	hc.lpfnWndProc = (WNDPROC)CHyperlink::WndProc;
	hc.cbClsExtra = 0;
	hc.cbWndExtra = sizeof(CHyperlink*);
	hc.hInstance = NULL;
	hc.hIcon = NULL;
	hc.hCursor = NULL;
	hc.hbrBackground = NULL;
	hc.lpszMenuName = NULL;
	hc.lpszClassName = STATIC_HYPER_WINDOW_CLASS;
	RegisterClass(&hc);

	CHyperlink::s_hHandCursor = ::LoadCursor(NULL, MAKEINTRESOURCE(IDC_HAND));
}

CHyperlink::StaticInitializer CHyperlink::s_oStaticInitializer;
HCURSOR CHyperlink::s_hHandCursor = NULL;
CHyperlink::CHyperlink(std::ofstream_t& oLogStream, const CTextAttributes& oTextAttributes)
: m_hwnd(NULL), 
m_bMouseInWindow(false),
m_oTextAttributes(oTextAttributes),
m_eCommandType(ctNone),
m_sCommand(_T("")),
m_sCommandParameters(_T("")),
m_oLogStream(oLogStream)
{}

CHyperlink::CHyperlink(std::ofstream_t& oLogStream, const CTextAttributes& oTextAttributes, CommandType eCommandType, const std::string_t& sCommand, const std::string_t& sCommandParameters)
: m_hwnd(NULL), 
m_bMouseInWindow(false),
m_oTextAttributes(oTextAttributes),
m_eCommandType(eCommandType),
m_sCommand(sCommand),
m_sCommandParameters(sCommandParameters),
m_oLogStream(oLogStream)
{
}

CHyperlink::~CHyperlink()
{
	if (m_hwnd)
	{
		::DestroyWindow(m_hwnd);
		m_hwnd = NULL;
	}
}

bool CHyperlink::Create(int nRresourceId, HWND hwndParent)
{
	HWND hwndOld = ::GetDlgItem(hwndParent, nRresourceId);
	if (hwndOld != NULL)
	{
		RECT rect = {0}; 
		TCHAR tszText[256];

		::GetWindowText(hwndOld, tszText, sizeof(tszText));
		::GetWindowRect(hwndOld, &rect);

		//GetWindowRect return bounding box in screen coordinates.
		POINT pos = {0};
		pos.x = rect.left;
		pos.y = rect.top;
		
		//calculate them down to client coordinates of the according dialog box...
		ScreenToClient(hwndParent, &pos);
		rect.left = pos.x;
		rect.top = pos.y;

		//finally, destroy the old label
		if (Create(rect, tszText, hwndParent))
		{
			::DestroyWindow(hwndOld);
		}
	}
	return (m_hwnd != NULL);
}

bool CHyperlink::Create(RECT rect, const TCHAR* tszText, HWND hwndParent)
{
	if (tszText != NULL)
	{
		m_sText = tszText;
	}

	m_hwnd = ::CreateWindow( STATIC_HYPER_WINDOW_CLASS, m_sText.c_str(), WS_CHILD | WS_VISIBLE, 
		rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top, 
		hwndParent, NULL, NULL, NULL);

	::SetWindowLong(m_hwnd, GWL_USERDATA, (LONG)this);

	return (m_hwnd != NULL);
}

bool CHyperlink::Create(int x, int y, const TCHAR* tszText, HWND hwndParent)
{
	RECT rect = {0}; 
	rect.left = x;
	rect.top = y;
	rect.right = x+1;
	rect.bottom = y+1;
	return Create(rect, tszText, hwndParent);
}

int CHyperlink::WndProc(HWND hwnd, WORD wMsg, WPARAM wParam, LPARAM lParam)
{
	CHyperlink* pHyperlink = (CHyperlink*)GetWindowLong(hwnd, GWL_USERDATA);
	switch (wMsg)  
	{
	case WM_LBUTTONDOWN:
		switch(pHyperlink->m_eCommandType)
		{
		case ctNone:
			break;

		case ctShellExecOpen:
			{
				pHyperlink->m_oLogStream << _T("Performing 'ShellExecute open' of '") << pHyperlink->m_sCommand << _T("' with parameters '") << pHyperlink->m_sCommandParameters << _T("'.") << std::endl;
				if (((UINT)::ShellExecute(NULL, _T("open"), pHyperlink->m_sCommand.c_str(), pHyperlink->m_sCommandParameters.c_str(), NULL, SW_SHOWNORMAL)) <= 32)
				{
					MessageBeep(0);
				}
			}
			break;

		case ctExit:
			PostMessage(GetParent(hwnd), WM_CLOSE, 0, 0);
			break;
		}
		break;

	case WM_PAINT:
		{
			HDC hDC; 
			PAINTSTRUCT ps = {0};
			hDC = ::BeginPaint(hwnd, &ps);
			if (pHyperlink == NULL)
			{
				return 0;
			}

			HFONT font = ::CreateFont( pHyperlink->m_oTextAttributes.GetHeight(), //height
				pHyperlink->m_oTextAttributes.GetWidth(), //average char width
				0, //angle of escapement
				0, //base-line orientation angle
				pHyperlink->m_oTextAttributes.GetWeight(),	//font weight
				pHyperlink->m_oTextAttributes.GetItalic(),		//italic
				pHyperlink->m_oTextAttributes.GetUnderline(),		//underline
				FALSE,		//strikeout
				ANSI_CHARSET,			//charset identifier
				OUT_DEFAULT_PRECIS,		//ouput precision
				CLIP_DEFAULT_PRECIS,	//clipping precision
				DEFAULT_QUALITY,		//output quality
				DEFAULT_PITCH,			//pitch and family
				pHyperlink->m_oTextAttributes.GetFace().c_str());

			::SelectObject(hDC, font);
			::SetTextColor(hDC, RGB(0, 0, 0));
			::SetTextColor(hDC, pHyperlink->m_oTextAttributes.GetTextColor());

			// set background color
			::SetBkMode(hDC, OPAQUE);
			if(pHyperlink->m_eCommandType != ctNone && pHyperlink->m_bMouseInWindow)
			{
				::SetBkColor(hDC, pHyperlink->m_oTextAttributes.GetHoverBackgroundColor());
			}
			else
			{
				::SetBkColor(hDC, pHyperlink->m_oTextAttributes.GetBackgroundColor());
			}


			// adjust item size based on text
			int iItemLength = _tcslen(pHyperlink->m_sText.c_str());
			SIZE extentSize = {0};
			GetTextExtentPoint32(hDC, pHyperlink->m_sText.c_str(), iItemLength, &extentSize);

			RECT rectToCenter = {0};
			::GetWindowRect(hwnd, &rectToCenter);
			if((rectToCenter.right - rectToCenter.left) != extentSize.cx || 
				(rectToCenter.bottom - rectToCenter.top) != extentSize.cy)
			{
				::SetWindowPos(hwnd, NULL, -1, -1, extentSize.cx, extentSize.cy, SWP_NOMOVE | SWP_NOZORDER | SWP_NOACTIVATE);
			}


			RECT rect = {0};
			::GetClientRect(hwnd, &rect);

			::DrawText(hDC, pHyperlink->m_sText.c_str(), pHyperlink->m_sText.length(), &rect, DT_VCENTER | DT_CENTER);
			::DeleteObject(font);

			::EndPaint(hwnd, &ps);

			return TRUE;
		}

	case WM_SETCURSOR:
		{
			if(pHyperlink->m_eCommandType != ctNone)
			{
				if (CHyperlink::s_hHandCursor)
				{
					::SetCursor(CHyperlink::s_hHandCursor);
				}
			}
		}
		break;

	case WM_MOUSEMOVE: 
		{             
			if (!pHyperlink->m_bMouseInWindow) 
			{ 
				pHyperlink->m_bMouseInWindow = true; 
				TRACKMOUSEEVENT tme; 
				tme.cbSize = sizeof(tme); 
				tme.dwFlags = TME_LEAVE; 
				tme.hwndTrack = hwnd; 
				TrackMouseEvent(&tme); 
				InvalidateRect(hwnd, NULL, FALSE);
			}
		} 
		break; 

	case WM_MOUSELEAVE: 
		{ 
			if(pHyperlink->m_bMouseInWindow)
			{
				pHyperlink->m_bMouseInWindow = false; 
				InvalidateRect(hwnd, NULL, FALSE);
			}
		} 
		break;

	default:
		DefWindowProc(hwnd, wMsg, wParam, lParam);
	}

	return TRUE;
}