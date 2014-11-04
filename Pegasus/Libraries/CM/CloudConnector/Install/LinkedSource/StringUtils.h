#pragma once
#include <locale>
#include <string>
#include <vector>
#include <regex>
#include <sstream>
#include <fstream>

#ifdef _UNICODE
namespace std
{
	typedef std::ctype<wchar_t> ctype_t;
	typedef std::wstring        string_t;
	typedef std::wistream       istream_t;
	typedef std::wostream       ostream_t;
	typedef std::wistringstream istringstream_t;
	typedef std::wostringstream ostringstream_t;
	typedef std::wofstream      ofstream_t;
	typedef std::wifstream      ifstream_t;
	typedef std::wfstream       fstream_t;
	#define cerr_t              std::wcerr
	#define cout_t              std::wcout
	namespace tr1
	{
		typedef std::tr1::basic_regex<wchar_t> regex_t;
		typedef std::tr1::match_results<const wchar_t*> cmatch_t;
		typedef std::tr1::match_results<std::wstring::const_iterator> smatch_t;
	};
};
#else
namespace std 
{
	typedef std::ctype<char>    ctype_t;
	typedef std::string_t_t     string_t;
	typedef std::istream        istream_t;
	typedef std::ostream        ostream_t;
	typedef std::istringstream  istringstream_t;
	typedef std::ostringstream  ostringstream_t;
	typedef std::ofstream       ofstream_t;
	typedef std::ifstream       ifstream_t;
	typedef std::fstream        fstream_t;
	#define cerr_t              std::cerr
	#define cout_t              std::cout
	namespace tr1
	{
		typedef std::tr1::basic_regex<char> regex_t;
		typedef std::tr1::match_results<const char*> cmatch_t;
		typedef std::tr1::match_results<std::string::const_iterator> smatch_t;
	};
};
#endif

typedef std::vector<std::string_t> StringVector;

extern std::vector<std::string_t> Tokenize(const std::string_t& str, const std::string_t& delims=_T(", \t"));
extern std::string_t ToLower(const std::string_t& str);
extern std::string_t TrimSpaces(const std::string_t& str);
extern std::string_t Trim(const std::string_t& str, const std::string_t& trim);
extern std::string_t TrimStart(const std::string_t& str, const std::string_t& trim);
extern std::string_t TrimEnd(const std::string_t& str, const std::string_t& trim);
extern std::string_t Replace(const std::string_t& str, const std::string_t& from, const std::string_t& to);
extern bool StartsWith(const std::string_t& str, const std::string_t& value);
extern bool EndsWith(const std::string_t& str, const std::string_t& value);
extern std::string_t Format(const TCHAR* fmt, ...);
extern DWORD StringToDWORD(const std::string_t& str, int base);
