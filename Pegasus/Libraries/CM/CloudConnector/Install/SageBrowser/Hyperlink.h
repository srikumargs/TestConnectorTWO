#pragma once
#include <string>
#include "TextAttributes.h"
#include "DynamicTextInfo.h"

class CHyperlink  
{
public:
	CHyperlink(std::ofstream_t& oLogStream, const CTextAttributes& oTextAttributes);
	CHyperlink(std::ofstream_t& oLogStream, const CTextAttributes& oTextAttributes, CommandType eCommandType, const std::string_t& sCommand, const std::string_t& sCommandParameters);
	virtual ~CHyperlink();

	bool Create(int nResourceId, HWND hwndParent);
	bool Create(RECT rect, const TCHAR* tszText, HWND hwndParent);
	bool Create(int x, int y, const TCHAR* tszText, HWND hwndParent);

private:
	class StaticInitializer
	{
	public:
		StaticInitializer();
	};
	friend class StaticInitializer;

	std::ofstream_t& m_oLogStream;
	HWND m_hwnd;
	std::string_t m_sText;
	bool m_bMouseInWindow;
	const CTextAttributes& m_oTextAttributes;
	CommandType m_eCommandType;
	std::string_t m_sCommand;
	std::string_t m_sCommandParameters;

	static StaticInitializer s_oStaticInitializer;
	static HCURSOR s_hHandCursor;
	static int WndProc(HWND hwnd,WORD wMsg,WPARAM wParam,LPARAM lParam);

	// Disallow copying and assignment
	CHyperlink(const CHyperlink&);
	CHyperlink& operator=(const CHyperlink&);
};
