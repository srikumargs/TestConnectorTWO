#pragma once

#include "..\LinkedSource\StringUtils.h"
#include <list>

void DeleteFiles(std::ofstream_t& oLogStream, const std::list<std::string_t>& fileNames);
BOOL DeleteFile(std::ofstream_t& oLogStream, LPTSTR tszFile);
BOOL DeleteDirectory(std::ofstream_t& oLogStream, LPCTSTR tszDirectory);
BOOL DeleteDirectoryRecursive(std::ofstream_t& oLogStream, LPTSTR tszDirectory);
BOOL DeleteRegistryKey(std::ofstream_t& oLogStream, HKEY hKey, LPTSTR tszSubKey );
std::list<std::string_t> FindFiles(std::ofstream_t& oLogStream, LPCTSTR lpctszPath, LPCTSTR lpctszFileSpec);
void CopyFiles(std::ofstream_t& oLogStream, const std::list<std::string_t>& fileNames, const std::string_t& dPath);
BOOL SageCopyFile(std::ofstream_t& oLogStream, LPCTSTR tszSourceFile, LPCTSTR tszDestFile);
BOOL MoveFileReplaceExisting(std::ofstream_t& oLogStream, LPTSTR tszSourceFile, LPTSTR tszDestFile);
BOOL DirectoryExists(std::ofstream_t& oLogStream, LPCTSTR szPath);
BOOL FileExists(std::ofstream_t& oLogStream, LPCTSTR szPath);
BOOL MoveDirectoryRecursive(std::ofstream_t& oLogStream, LPCTSTR szOldDir, LPCTSTR szNewDir);

