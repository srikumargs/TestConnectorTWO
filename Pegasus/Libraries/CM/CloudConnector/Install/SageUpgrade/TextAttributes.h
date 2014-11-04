#pragma once
#include "..\LinkedSource\StringUtils.h"

class CTextAttributes
{
public:
	CTextAttributes()
	{}

	CTextAttributes(int nHeght, int nWidth, int nWeight, bool bItalic, bool bUnderline, const std::string_t& sFace, DWORD dwTextColor, DWORD dwBackgroundColor, DWORD dwHoverBackgroundColor);

	int GetHeight() const
	{ return m_nHeight; }

	int GetWidth() const
	{ return m_nWidth; }

	int GetWeight() const
	{ return m_nWeight; }

	bool GetItalic() const
	{ return m_bItalic; }

	bool GetUnderline() const
	{ return m_bUnderline; }

	const std::string_t& GetFace() const
	{ return m_sFace; }

	DWORD GetTextColor() const
	{ return m_dwTextColor; }

	DWORD GetBackgroundColor() const
	{ return m_dwBackgroundColor; }

	DWORD GetHoverBackgroundColor() const
	{ return m_dwHoverBackgroundColor; }

private:
	int m_nHeight;
	int m_nWidth;
	int m_nWeight;
	bool m_bItalic;
	bool m_bUnderline;
	std::string_t m_sFace;
	DWORD m_dwTextColor;
	DWORD m_dwBackgroundColor;
	DWORD m_dwHoverBackgroundColor;
};
