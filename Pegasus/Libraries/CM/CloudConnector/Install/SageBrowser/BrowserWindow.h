#pragma once
#include <iostream>
#include <string>
#include "..\LinkedSource\StringUtils.h"
#include "..\LinkedSource\SharedPtr.h"
#include "Configuration.h"
#include "Hyperlink.h"

class CBrowserWindow
{
public:
	CBrowserWindow(HINSTANCE hInstance, const CConfiguration& oConfig);

	void Show();

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

	typedef std::vector<CSharedPtr<CHyperlink>> HyperlinkContainer;
	HyperlinkContainer m_hyperlinks;

	// Disallow copying and assignment
	CBrowserWindow(const CBrowserWindow&);
	CBrowserWindow& operator=(const CBrowserWindow&);
};
