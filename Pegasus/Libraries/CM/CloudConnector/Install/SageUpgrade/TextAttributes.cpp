#include "StdAfx.h"
#include "TextAttributes.h"

CTextAttributes::CTextAttributes(int nHeight, int nWidth, int nWeight, bool bItalic, bool bUnderline, const std::string_t& sFace, DWORD dwTextColor, DWORD dwBackgroundColor, DWORD dwHoverBackgroundColor)
: m_nHeight(nHeight),
	m_nWidth(nWidth),
	m_nWeight(nWeight),
	m_bItalic(bItalic),
	m_bUnderline(bUnderline),
	m_sFace(sFace),
	m_dwTextColor(dwTextColor),
	m_dwBackgroundColor(dwBackgroundColor),
	m_dwHoverBackgroundColor(dwHoverBackgroundColor)
{
}
